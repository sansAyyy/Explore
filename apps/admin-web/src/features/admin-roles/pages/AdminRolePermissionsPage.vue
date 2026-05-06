<script setup lang="ts">
import { ArrowLeft, RefreshRight } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { computed, nextTick, onMounted, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import {
  assignAdminRolePermissions,
  getAdminRoleById,
  getAdminRolePermissions,
} from '@/features/admin-roles/api/adminRolesApi'
import type {
  AdminRoleDetailResponse,
  AssignedRolePermissionResponse,
} from '@/features/admin-roles/types/adminRoles'
import {
  getAdminPermissionDescendants,
  getAdminPermissionRoots,
} from '@/features/admin-permissions/api/adminPermissionsApi'
import {
  buildRoleSelectablePermissionTree,
  filterAdminPermissionTree,
  mapAdminPermissionTreeNode,
} from '@/features/admin-permissions/models/adminPermissionTree'
import type { AdminPermissionTreeNode } from '@/features/admin-permissions/types/adminPermissions'
import { extractErrorMessage } from '@/shared/api/error'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()

const role = ref<AdminRoleDetailResponse | null>(null)
const permissionForest = ref<AdminPermissionTreeNode[]>([])
const assignedPermissions = ref<AssignedRolePermissionResponse[]>([])
const selectedPermissionIds = ref<string[]>([])
const isLoading = ref(false)
const isSubmitting = ref(false)

const treeRef = ref<{
  setCheckedKeys: (keys: string[], leafOnly?: boolean) => void
  getCheckedKeys: (leafOnly?: boolean) => Array<string | number>
  store?: {
    nodesMap?: Record<string, { expanded: boolean }>
  }
} | null>(null)

const filters = reactive({
  keyword: '',
  resourceType: 'all' as const,
  onlySelected: false,
})

const roleId = computed(() => String(route.params.roleId ?? ''))
const canAssignPermissions = computed(() =>
  authStore.hasButtonPermission('admin_roles.assign_permissions'),
)
const selectedCount = computed(() => selectedPermissionIds.value.length)
const filteredPermissionForest = computed(() =>
  filterAdminPermissionTree(
    permissionForest.value,
    {
      keyword: filters.keyword,
      resourceType: filters.resourceType,
      status: 'all',
      onlySelected: filters.onlySelected,
    },
    selectedPermissionIds.value,
  ),
)
const assignablePermissionForest = computed(() =>
  buildRoleSelectablePermissionTree(filteredPermissionForest.value),
)
const inactiveAssignedPermissions = computed(() =>
  assignedPermissions.value.filter((permission) => !permission.isActive),
)

function getCheckedPermissionIds() {
  return (treeRef.value?.getCheckedKeys(false) ?? []).map((key) => String(key))
}

function syncTreeCheckedKeys() {
  nextTick(() => {
    treeRef.value?.setCheckedKeys(selectedPermissionIds.value)
  })
}

function expandTree(expanded: boolean) {
  const nodesMap = treeRef.value?.store?.nodesMap ?? {}

  for (const node of Object.values(nodesMap)) {
    node.expanded = expanded
  }
}

function handleTreeCheck() {
  selectedPermissionIds.value = getCheckedPermissionIds()
}

async function loadPage() {
  if (!roleId.value) {
    return
  }

  isLoading.value = true

  try {
    const [roleDetail, rolePermissions, roots] = await Promise.all([
      getAdminRoleById(roleId.value),
      getAdminRolePermissions(roleId.value),
      getAdminPermissionRoots({
        isActive: true,
      }),
    ])

    const descendants = await Promise.all(
      roots.map((rootPermission) =>
        getAdminPermissionDescendants(rootPermission.id, {
          isActive: true,
        }),
      ),
    )

    role.value = roleDetail
    assignedPermissions.value = rolePermissions.permissions
    const nextPermissionForest = descendants.map((tree) => mapAdminPermissionTreeNode(tree))

    permissionForest.value = nextPermissionForest
    selectedPermissionIds.value = rolePermissions.permissions
      .filter((permission) => permission.isActive)
      .map((permission) => permission.id)

    syncTreeCheckedKeys()
  } catch (error) {
    role.value = null
    assignedPermissions.value = []
    permissionForest.value = []
    selectedPermissionIds.value = []
    ElMessage.error(extractErrorMessage(error, '加载角色权限配置失败'))
  } finally {
    isLoading.value = false
  }
}

async function handleSubmit() {
  if (!role.value || !canAssignPermissions.value) {
    return
  }

  isSubmitting.value = true

  try {
    await assignAdminRolePermissions(role.value.id, {
      permissionIds: [...selectedPermissionIds.value],
    })

    if (authStore.roleCodes.includes(role.value.code)) {
      try {
        await authStore.fetchCurrentAuthorization()
      } catch {
        ElMessage.warning('权限已保存，但当前权限上下文刷新失败，请刷新页面后重试')
      }
    }

    ElMessage.success('保存角色权限成功')
    await router.push({
      name: 'admin-roles',
    })
  } catch (error) {
    ElMessage.error(extractErrorMessage(error, '保存角色权限失败'))
  } finally {
    isSubmitting.value = false
  }
}

watch(
  () => [filteredPermissionForest.value, selectedPermissionIds.value.join(',')] as const,
  () => {
    syncTreeCheckedKeys()
  },
)

onMounted(() => {
  void loadPage()
})
</script>

<template>
  <div class="admin-role-permissions-page">
    <section class="admin-role-permissions-page__hero">
      <div class="admin-role-permissions-page__hero-main">
        <p class="admin-role-permissions-page__eyebrow">Role Permissions</p>
        <h1 class="admin-role-permissions-page__title">角色权限配置</h1>
      </div>

      <div class="admin-role-permissions-page__hero-actions">
        <el-button class="admin-role-permissions-page__hero-button" :icon="ArrowLeft" @click="router.push({ name: 'admin-roles' })">
          返回角色列表
        </el-button>
        <el-button class="admin-role-permissions-page__hero-button" :icon="RefreshRight" @click="loadPage">
          刷新
        </el-button>
      </div>
    </section>

    <section class="admin-role-permissions-page__section">
      <el-card shadow="never" class="admin-role-permissions-page__context-card">
        <div class="admin-role-permissions-page__context">
          <div class="admin-role-permissions-page__context-item">
            <div class="admin-role-permissions-page__context-label">角色名称</div>
            <div class="admin-role-permissions-page__context-value">
              {{ role?.name || '-' }}
            </div>
          </div>
          <div class="admin-role-permissions-page__context-item">
            <div class="admin-role-permissions-page__context-label">角色编码</div>
            <div class="admin-role-permissions-page__context-value admin-role-permissions-page__context-value--mono">
              {{ role?.code || '-' }}
            </div>
          </div>
          <div class="admin-role-permissions-page__context-item">
            <div class="admin-role-permissions-page__context-label">状态</div>
            <div class="admin-role-permissions-page__context-value">
              <el-tag :type="role?.isActive ? 'success' : 'info'">
                {{ role?.isActive ? '启用' : '禁用' }}
              </el-tag>
            </div>
          </div>
          <div class="admin-role-permissions-page__context-item">
            <div class="admin-role-permissions-page__context-label">已选权限</div>
            <div class="admin-role-permissions-page__context-value">
              {{ selectedCount }}
            </div>
          </div>
        </div>
      </el-card>
    </section>

    <el-alert
      v-if="inactiveAssignedPermissions.length > 0"
      type="warning"
      :closable="false"
      class="admin-role-permissions-page__alert"
      title="当前角色存在已停用权限，保存后这些权限将被移除。"
    />

    <section class="admin-role-permissions-page__layout">
      <el-card shadow="never" class="admin-role-permissions-page__card admin-role-permissions-page__card--tree">
        <template #header>
          <div class="admin-role-permissions-page__toolbar">
            <div class="admin-role-permissions-page__toolbar-main">
              <div class="admin-role-permissions-page__card-title">权限树</div>
              <div class="admin-role-permissions-page__card-meta">
                可按名称、编码筛选，勾选结果即时汇总
              </div>
            </div>

            <div class="admin-role-permissions-page__toolbar-controls">
              <el-input
                v-model="filters.keyword"
                clearable
                placeholder="按权限名称、编码或描述搜索"
                class="admin-role-permissions-page__field admin-role-permissions-page__field--keyword"
              />
              <el-checkbox v-model="filters.onlySelected" class="admin-role-permissions-page__checkbox">
                仅看已选
              </el-checkbox>
              <div class="admin-role-permissions-page__toolbar-actions">
                <el-button class="admin-role-permissions-page__toolbar-button" @click="expandTree(true)">
                  展开全部
                </el-button>
                <el-button class="admin-role-permissions-page__toolbar-button" @click="expandTree(false)">
                  收起全部
                </el-button>
              </div>
            </div>
          </div>
        </template>

        <div v-loading="isLoading" class="admin-role-permissions-page__tree-shell">
          <div v-if="!isLoading && filteredPermissionForest.length === 0" class="admin-role-permissions-page__empty">
            <el-empty description="暂无可配置的启用权限" />
          </div>

          <div v-else class="admin-role-permissions-page__tree-container">
            <el-tree
              ref="treeRef"
              node-key="id"
              show-checkbox
              default-expand-all
              :data="assignablePermissionForest"
              :props="{ children: 'children', disabled: 'disabled' }"
              @check="handleTreeCheck"
            >
              <template #default="{ data }">
                <div class="admin-role-permissions-page__tree-node">
                  <div class="admin-role-permissions-page__tree-node-main">
                    <div class="admin-role-permissions-page__tree-node-title">
                      {{ data.name }}
                    </div>
                    <div class="admin-role-permissions-page__tree-node-code">
                      {{ data.code }}
                    </div>
                  </div>
                  <el-tag size="small" type="info">{{ data.resourceTypeLabel }}</el-tag>
                </div>
              </template>
            </el-tree>
          </div>
        </div>

        <div class="admin-role-permissions-page__footer">
          <el-button class="admin-role-permissions-page__footer-button" @click="router.push({ name: 'admin-roles' })">
            取消
          </el-button>
          <el-button
            type="primary"
            class="admin-role-permissions-page__footer-button admin-role-permissions-page__footer-button--primary"
            :disabled="!canAssignPermissions"
            :loading="isSubmitting"
            @click="handleSubmit"
          >
            保存权限
          </el-button>
        </div>
      </el-card>
    </section>
  </div>
</template>

<style scoped>
.admin-role-permissions-page {
  display: grid;
  grid-template-rows: auto auto auto minmax(0, 1fr);
  gap: 10px;
  min-height: 0;
  height: 100%;
  color: #334155;
  overflow: hidden;
}

.admin-role-permissions-page__hero {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: center;
  min-width: 0;
  padding: 0 2px 4px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.9);
}

.admin-role-permissions-page__hero-main {
  display: grid;
  gap: 2px;
  min-width: 0;
}

.admin-role-permissions-page__eyebrow {
  margin: 0;
  color: #64748b;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.admin-role-permissions-page__title {
  margin: 0;
  color: #0f172a;
  font-size: clamp(22px, 2vw, 26px);
  font-weight: 750;
  line-height: 1.04;
  letter-spacing: -0.03em;
}

.admin-role-permissions-page__hero-actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.admin-role-permissions-page__hero-button,
.admin-role-permissions-page__toolbar-button,
.admin-role-permissions-page__footer-button {
  min-height: 38px;
  border-radius: 10px;
  font-weight: 600;
  transition:
    border-color 0.2s ease,
    background-color 0.2s ease,
    color 0.2s ease,
    box-shadow 0.2s ease;
}

.admin-role-permissions-page__hero-button,
.admin-role-permissions-page__toolbar-button {
  border-color: rgba(148, 163, 184, 0.24);
  background: rgba(248, 250, 252, 0.92);
  color: #334155;
}

.admin-role-permissions-page__section,
.admin-role-permissions-page__layout {
  min-width: 0;
}

.admin-role-permissions-page__context-card,
.admin-role-permissions-page__card {
  border: 1px solid rgba(148, 163, 184, 0.24);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.96);
  box-shadow:
    0 12px 24px rgba(15, 23, 42, 0.04),
    0 4px 10px rgba(59, 130, 246, 0.05);
  backdrop-filter: blur(10px);
}

.admin-role-permissions-page__context-card :deep(.el-card__body) {
  padding: 12px 14px;
}

.admin-role-permissions-page__card {
  display: flex;
  flex-direction: column;
  min-height: 0;
  height: 100%;
}

.admin-role-permissions-page__card :deep(.el-card__header) {
  padding: 10px 14px 0;
  border-bottom: none;
}

.admin-role-permissions-page__card :deep(.el-card__body) {
  display: flex;
  flex: 1 1 auto;
  flex-direction: column;
  min-height: 0;
  padding: 8px 14px 12px;
}

.admin-role-permissions-page__context {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
}

.admin-role-permissions-page__context-item {
  padding: 10px 12px;
  border: 1px solid rgba(226, 232, 240, 0.92);
  border-radius: 12px;
  background: linear-gradient(180deg, rgba(248, 250, 252, 0.76), rgba(255, 255, 255, 0.96));
}

.admin-role-permissions-page__context-label {
  color: #64748b;
  font-size: 11px;
  font-weight: 600;
  line-height: 1.35;
}

.admin-role-permissions-page__context-value {
  margin-top: 6px;
  color: #0f172a;
  font-size: 14px;
  font-weight: 700;
  line-height: 1.35;
}

.admin-role-permissions-page__context-value--mono {
  font-family: Consolas, 'Courier New', monospace;
  font-size: 13px;
  font-weight: 600;
}

.admin-role-permissions-page__context-value :deep(.el-tag) {
  min-height: 26px;
  padding: 0 10px;
  border-radius: 999px;
  font-weight: 600;
}

.admin-role-permissions-page__alert {
  border-radius: 14px;
  overflow: hidden;
}

.admin-role-permissions-page__alert :deep(.el-alert) {
  border: 1px solid rgba(245, 158, 11, 0.18);
}

.admin-role-permissions-page__layout {
  display: block;
  min-height: 0;
}

.admin-role-permissions-page__card--tree {
  min-width: 0;
  min-height: 0;
}

.admin-role-permissions-page__toolbar {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
  gap: 12px;
  align-items: center;
  padding-bottom: 4px;
}

.admin-role-permissions-page__toolbar-main {
  min-width: 0;
}

.admin-role-permissions-page__card-title {
  color: #0f172a;
  font-size: 13px;
  font-weight: 700;
  line-height: 1.25;
}

.admin-role-permissions-page__card-meta {
  margin-top: 2px;
  color: #64748b;
  font-size: 11px;
  line-height: 1.4;
}

.admin-role-permissions-page__toolbar-controls {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 10px;
  min-width: 0;
  flex-wrap: wrap;
}

.admin-role-permissions-page__field {
  width: 100%;
}

.admin-role-permissions-page__field--keyword {
  max-width: 340px;
}

.admin-role-permissions-page__field :deep(.el-input__wrapper) {
  min-height: 38px;
  padding: 0 10px;
  border-radius: 10px;
  background: rgba(248, 250, 252, 0.94);
  box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.18);
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.admin-role-permissions-page__field :deep(.el-input__inner) {
  color: #0f172a;
}

.admin-role-permissions-page__field :deep(.el-input__inner::placeholder) {
  color: #94a3b8;
}

.admin-role-permissions-page__field :deep(.el-input__wrapper:hover) {
  background: #ffffff;
  box-shadow: inset 0 0 0 1px rgba(100, 116, 139, 0.28);
}

.admin-role-permissions-page__field :deep(.el-input__wrapper.is-focus) {
  background: #ffffff;
  box-shadow:
    inset 0 0 0 1px rgba(59, 130, 246, 0.6),
    0 0 0 3px rgba(59, 130, 246, 0.1);
}

.admin-role-permissions-page__checkbox {
  min-height: 38px;
  padding: 0 4px;
  color: #334155;
  font-weight: 600;
}

.admin-role-permissions-page__checkbox :deep(.el-checkbox__label) {
  color: inherit;
}

.admin-role-permissions-page__toolbar-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.admin-role-permissions-page__tree-shell {
  display: flex;
  flex: 1 1 auto;
  flex-direction: column;
  min-height: 0;
}

.admin-role-permissions-page__tree-container,
.admin-role-permissions-page__empty {
  flex: 1 1 auto;
  min-height: 0;
  border: 1px solid rgba(148, 163, 184, 0.16);
  border-radius: 14px;
  background:
    linear-gradient(180deg, rgba(248, 250, 252, 0.92), rgba(255, 255, 255, 0.97));
}

.admin-role-permissions-page__tree-container {
  overflow: auto;
  padding: 8px;
}

.admin-role-permissions-page__tree-container :deep(.el-tree) {
  min-width: 0;
  background: transparent;
  color: #0f172a;
  --el-tree-node-hover-bg-color: rgba(239, 246, 255, 0.92);
}

.admin-role-permissions-page__tree-container :deep(.el-tree-node__content) {
  min-height: 44px;
  padding-right: 8px;
  border-radius: 10px;
}

.admin-role-permissions-page__tree-container :deep(.el-tree-node__content:hover) {
  background: rgba(239, 246, 255, 0.92);
}

.admin-role-permissions-page__tree-container :deep(.el-checkbox__input.is-checked .el-checkbox__inner),
.admin-role-permissions-page__tree-container :deep(.el-checkbox__input.is-indeterminate .el-checkbox__inner) {
  border-color: #2563eb;
  background-color: #2563eb;
}

.admin-role-permissions-page__tree-node {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  width: 100%;
  min-width: 0;
  padding: 4px 0;
}

.admin-role-permissions-page__tree-node-main {
  min-width: 0;
}

.admin-role-permissions-page__tree-node-title {
  color: #0f172a;
  font-weight: 600;
  line-height: 1.35;
}

.admin-role-permissions-page__tree-node-code {
  margin-top: 2px;
  color: #64748b;
  font-size: 11px;
  line-height: 1.35;
  word-break: break-all;
}

.admin-role-permissions-page__tree-node :deep(.el-tag) {
  flex: none;
  min-height: 24px;
  padding: 0 8px;
  border-radius: 999px;
}

.admin-role-permissions-page__empty {
  display: grid;
  place-items: center;
  padding: 12px;
}

.admin-role-permissions-page__empty :deep(.el-empty__description p) {
  color: #64748b;
}

.admin-role-permissions-page__footer {
  display: flex;
  flex: none;
  justify-content: flex-end;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 8px;
  padding-top: 8px;
  border-top: 1px solid rgba(226, 232, 240, 0.92);
}

.admin-role-permissions-page__footer-button {
  min-width: 96px;
}

.admin-role-permissions-page__footer-button--primary {
  border-color: transparent;
  background: linear-gradient(135deg, #1e40af 0%, #2563eb 100%);
  box-shadow: 0 8px 18px rgba(37, 99, 235, 0.16);
}

@media (max-width: 1080px) {
  .admin-role-permissions-page {
    grid-template-rows: auto auto auto auto;
    height: auto;
    overflow: visible;
  }

  .admin-role-permissions-page__card {
    height: auto;
  }

  .admin-role-permissions-page__tree-container {
    min-height: 360px;
  }
}

@media (max-width: 920px) {
  .admin-role-permissions-page {
    gap: 12px;
  }

  .admin-role-permissions-page__hero {
    align-items: flex-start;
    flex-direction: column;
    padding-bottom: 6px;
  }

  .admin-role-permissions-page__hero-actions {
    justify-content: flex-start;
  }

  .admin-role-permissions-page__context {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .admin-role-permissions-page__toolbar {
    grid-template-columns: 1fr;
  }

  .admin-role-permissions-page__toolbar-controls {
    justify-content: flex-start;
  }
}

@media (max-width: 640px) {
  .admin-role-permissions-page {
    gap: 10px;
  }

  .admin-role-permissions-page__title {
    font-size: clamp(20px, 7vw, 24px);
  }

  .admin-role-permissions-page__context {
    grid-template-columns: 1fr;
  }

  .admin-role-permissions-page__context-card :deep(.el-card__body) {
    padding: 10px 12px;
  }

  .admin-role-permissions-page__card :deep(.el-card__header) {
    padding: 10px 12px 0;
  }

  .admin-role-permissions-page__card :deep(.el-card__body) {
    padding: 8px 12px 12px;
  }

  .admin-role-permissions-page__hero-button,
  .admin-role-permissions-page__toolbar-button,
  .admin-role-permissions-page__footer-button {
    flex: 1 1 140px;
  }

  .admin-role-permissions-page__field--keyword {
    max-width: none;
  }
}
</style>
