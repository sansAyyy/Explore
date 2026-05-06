<script setup lang="ts">
import { ElMessage } from 'element-plus'
import { computed, onMounted, reactive, ref } from 'vue'

import {
  deleteAdminPermission,
  disableAdminPermission,
  enableAdminPermission,
  getAdminPermissionDescendants,
  getAdminPermissionRoots,
} from '@/features/admin-permissions/api/adminPermissionsApi'
import AdminPermissionUpsertDrawer from '@/features/admin-permissions/components/drawers/AdminPermissionUpsertDrawer.vue'
import {
  buildParentSelectableTree,
  collectAdminPermissionSubtreeIds,
  createDefaultAdminPermissionTreeFilters,
  filterAdminPermissionTree,
  flattenAdminPermissionTree,
  mapAdminPermissionTreeNode,
} from '@/features/admin-permissions/models/adminPermissionTree'
import type {
  AdminPermissionTreeFilterState,
  AdminPermissionTreeNode,
} from '@/features/admin-permissions/types/adminPermissions'
import { extractErrorMessage } from '@/shared/api/error'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const permissions = ref<AdminPermissionTreeNode[]>([])
const isLoading = ref(false)
const expandAll = ref(true)
const tableRenderKey = ref(0)
const filters = reactive<AdminPermissionTreeFilterState>(createDefaultAdminPermissionTreeFilters())

const drawerState = reactive<{
  initialParentId: string | null
  isOpen: boolean
  mode: 'create' | 'edit'
  permission: AdminPermissionTreeNode | null
}>({
  initialParentId: null,
  isOpen: false,
  mode: 'create',
  permission: null,
})

const actionState = reactive<{
  action: 'enable' | 'disable' | 'delete' | null
  permissionId: string | null
}>({
  action: null,
  permissionId: null,
})

const canCreateAdminPermission = computed(() =>
  authStore.hasButtonPermission('admin_permissions.create'),
)
const canUpdateAdminPermission = computed(() =>
  authStore.hasButtonPermission('admin_permissions.update'),
)
const canDeleteAdminPermission = computed(() =>
  authStore.hasButtonPermission('admin_permissions.delete'),
)

const filteredPermissions = computed(() =>
  filterAdminPermissionTree(permissions.value, filters, []),
)

const permissionLookup = computed(() => {
  const entries = flattenAdminPermissionTree(permissions.value).map((node) => [node.id, node] as const)
  return new Map(entries)
})

const parentOptions = computed(() => {
  if (!drawerState.permission) {
    return buildParentSelectableTree(permissions.value)
  }

  const fullPermission = permissionLookup.value.get(drawerState.permission.id)
  if (!fullPermission) {
    return buildParentSelectableTree(permissions.value)
  }

  return buildParentSelectableTree(
    permissions.value,
    collectAdminPermissionSubtreeIds(fullPermission),
  )
})

function bumpTableKey() {
  tableRenderKey.value += 1
}

function isRowPending(permissionId: string, action?: 'enable' | 'disable' | 'delete') {
  if (actionState.permissionId !== permissionId) {
    return false
  }

  return action ? actionState.action === action : true
}

async function loadPermissions() {
  isLoading.value = true

  try {
    const roots = await getAdminPermissionRoots({})
    const trees = await Promise.all(roots.map((root) => getAdminPermissionDescendants(root.id, {})))

    permissions.value = trees.map((tree) => mapAdminPermissionTreeNode(tree))
    bumpTableKey()
  } catch (error) {
    permissions.value = []
    ElMessage.error(extractErrorMessage(error, '加载权限树失败'))
  } finally {
    isLoading.value = false
  }
}

function handleOpenCreateRootDrawer() {
  drawerState.mode = 'create'
  drawerState.permission = null
  drawerState.initialParentId = null
  drawerState.isOpen = true
}

function handleOpenCreateChildDrawer(row: AdminPermissionTreeNode) {
  drawerState.mode = 'create'
  drawerState.permission = null
  drawerState.initialParentId = row.id
  drawerState.isOpen = true
}

function handleOpenEditDrawer(row: AdminPermissionTreeNode) {
  drawerState.mode = 'edit'
  drawerState.permission = row
  drawerState.initialParentId = null
  drawerState.isOpen = true
}

async function handleAction(
  row: AdminPermissionTreeNode,
  action: 'enable' | 'disable' | 'delete',
) {
  actionState.permissionId = row.id
  actionState.action = action

  try {
    if (action === 'enable') {
      await enableAdminPermission(row.id)
      ElMessage.success('启用权限成功')
    } else if (action === 'disable') {
      await disableAdminPermission(row.id)
      ElMessage.success('禁用权限成功')
    } else {
      await deleteAdminPermission(row.id)
      ElMessage.success('删除权限成功')
    }

    await loadPermissions()
  } catch (error) {
    ElMessage.error(
      extractErrorMessage(
        error,
        action === 'delete'
          ? '删除权限失败'
          : action === 'enable'
            ? '启用权限失败'
            : '禁用权限失败',
      ),
    )
  } finally {
    actionState.permissionId = null
    actionState.action = null
  }
}

function handleExpandAll() {
  expandAll.value = true
  bumpTableKey()
}

function handleCollapseAll() {
  expandAll.value = false
  bumpTableKey()
}

onMounted(() => {
  void loadPermissions()
})
</script>

<template>
  <div class="admin-permissions-page">
    <section class="admin-permissions-page__hero">
      <p class="admin-permissions-page__eyebrow">Permissions Admin</p>
      <h1 class="admin-permissions-page__title">权限管理</h1>
    </section>

    <section class="admin-permissions-page__section">
      <el-card shadow="never" class="admin-permissions-page__filters-card">
        <template #header>
          <div class="admin-permissions-page__filters-header">
            <div class="admin-permissions-page__card-title">筛选与操作</div>
          </div>
        </template>

        <div class="admin-permissions-page__toolbar">
          <div class="admin-permissions-page__fields">
            <el-input
              v-model="filters.keyword"
              clearable
              placeholder="按权限名称、编码或描述搜索"
              class="admin-permissions-page__field admin-permissions-page__field--keyword"
            />

            <el-select
              v-model="filters.status"
              placeholder="请选择状态"
              class="admin-permissions-page__field admin-permissions-page__field--status"
            >
              <el-option label="全部" value="all" />
              <el-option label="启用" value="enabled" />
              <el-option label="禁用" value="disabled" />
            </el-select>

            <el-select
              v-model="filters.resourceType"
              placeholder="选择类型"
              class="admin-permissions-page__field admin-permissions-page__field--status"
            >
              <el-option label="全部类型" value="all" />
              <el-option label="页面权限" :value="1" />
              <el-option label="按钮权限" :value="2" />
              <el-option label="分组节点" :value="3" />
            </el-select>
          </div>

          <div class="admin-permissions-page__toolbar-actions">
            <el-button
              class="admin-permissions-page__button admin-permissions-page__button--ghost"
              @click="handleExpandAll"
            >
              展开全部
            </el-button>
            <el-button
              class="admin-permissions-page__button admin-permissions-page__button--ghost"
              @click="handleCollapseAll"
            >
              收起全部
            </el-button>
            <el-button
              v-if="canCreateAdminPermission"
              type="primary"
              class="admin-permissions-page__button admin-permissions-page__button--primary"
              @click="handleOpenCreateRootDrawer"
            >
              新增根权限
            </el-button>
          </div>
        </div>
      </el-card>
    </section>

    <section class="admin-permissions-page__section admin-permissions-page__section--table">
      <el-card shadow="never" class="admin-permissions-page__table-card">
        <template #header>
          <div class="admin-permissions-page__table-header">
            <div>
              <div class="admin-permissions-page__card-title">权限树</div>
              <p class="admin-permissions-page__card-meta">
                当前显示 {{ filteredPermissions.length }} 个根节点
              </p>
            </div>
          </div>
        </template>

        <el-skeleton :loading="isLoading" animated class="admin-permissions-page__skeleton-shell">
          <template #template>
            <div class="admin-permissions-page__skeleton">
              <el-skeleton-item
                v-for="index in 6"
                :key="index"
                variant="rect"
                class="admin-permissions-page__skeleton-row"
              />
            </div>
          </template>

          <template #default>
            <div v-if="filteredPermissions.length === 0" class="admin-permissions-page__empty">
              <el-empty description="暂无权限数据" />
            </div>

            <template v-else>
              <div class="admin-permissions-page__table-shell">
                <el-table
                  :key="tableRenderKey"
                  :data="filteredPermissions"
                  row-key="id"
                  height="100%"
                  :default-expand-all="expandAll"
                  :tree-props="{ children: 'children' }"
                >
                  <el-table-column prop="name" label="权限名称" min-width="240" />
                  <el-table-column prop="code" label="权限编码" min-width="240" />
                  <el-table-column label="权限类型" width="120">
                    <template #default="{ row }">
                      {{ row.resourceTypeLabel }}
                    </template>
                  </el-table-column>
                  <el-table-column prop="isActive" label="状态" width="100">
                    <template #default="{ row }">
                      <el-tag :type="row.isActive ? 'success' : 'info'">
                        {{ row.isActive ? '启用' : '禁用' }}
                      </el-tag>
                    </template>
                  </el-table-column>
                  <el-table-column
                    v-if="canCreateAdminPermission || canUpdateAdminPermission || canDeleteAdminPermission"
                    label="操作"
                    fixed="right"
                    width="320"
                  >
                    <template #default="{ row }">
                      <div class="admin-permissions-page__row-actions">
                        <el-button
                          v-if="canCreateAdminPermission"
                          link
                          type="primary"
                          :disabled="isRowPending(row.id)"
                          @click="handleOpenCreateChildDrawer(row)"
                        >
                          新增子权限
                        </el-button>

                        <el-button
                          v-if="canUpdateAdminPermission"
                          link
                          type="primary"
                          :disabled="isRowPending(row.id)"
                          @click="handleOpenEditDrawer(row)"
                        >
                          编辑
                        </el-button>

                        <el-popconfirm
                          v-if="canUpdateAdminPermission && !row.isActive"
                          title="确认启用该权限吗？"
                          confirm-button-text="确认"
                          cancel-button-text="取消"
                          :disabled="isRowPending(row.id)"
                          @confirm="handleAction(row, 'enable')"
                        >
                          <template #reference>
                            <el-button
                              link
                              type="success"
                              :loading="isRowPending(row.id, 'enable')"
                            >
                              启用
                            </el-button>
                          </template>
                        </el-popconfirm>

                        <el-popconfirm
                          v-if="canUpdateAdminPermission && row.isActive"
                          title="确认禁用该权限吗？"
                          confirm-button-text="确认"
                          cancel-button-text="取消"
                          :disabled="isRowPending(row.id)"
                          @confirm="handleAction(row, 'disable')"
                        >
                          <template #reference>
                            <el-button
                              link
                              type="warning"
                              :loading="isRowPending(row.id, 'disable')"
                            >
                              禁用
                            </el-button>
                          </template>
                        </el-popconfirm>

                        <el-popconfirm
                          v-if="canDeleteAdminPermission"
                          title="确认删除该权限吗？"
                          confirm-button-text="确认"
                          cancel-button-text="取消"
                          :disabled="isRowPending(row.id)"
                          @confirm="handleAction(row, 'delete')"
                        >
                          <template #reference>
                            <el-button
                              link
                              type="danger"
                              :loading="isRowPending(row.id, 'delete')"
                            >
                              删除
                            </el-button>
                          </template>
                        </el-popconfirm>
                      </div>
                    </template>
                  </el-table-column>
                </el-table>
              </div>
            </template>
          </template>
        </el-skeleton>
      </el-card>
    </section>

    <AdminPermissionUpsertDrawer
      v-model="drawerState.isOpen"
      :initial-parent-id="drawerState.initialParentId"
      :mode="drawerState.mode"
      :parent-options="parentOptions"
      :permission="drawerState.permission"
      @changed="loadPermissions"
    />
  </div>
</template>

<style scoped>
.admin-permissions-page {
  display: grid;
  grid-template-rows: auto auto minmax(0, 1fr);
  gap: 10px;
  min-height: 0;
  height: 100%;
  color: #334155;
  overflow: hidden;
}

.admin-permissions-page__hero {
  display: grid;
  gap: 2px;
  min-width: 0;
  padding: 0 2px 4px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.9);
}

.admin-permissions-page__eyebrow {
  margin: 0;
  color: #64748b;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.admin-permissions-page__title {
  margin: 0;
  color: #0f172a;
  font-size: clamp(22px, 2vw, 26px);
  font-weight: 750;
  line-height: 1.04;
  letter-spacing: -0.03em;
}

.admin-permissions-page__section {
  min-width: 0;
}

.admin-permissions-page__section--table {
  min-height: 0;
}

.admin-permissions-page__filters-card,
.admin-permissions-page__table-card {
  border: 1px solid rgba(148, 163, 184, 0.24);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.96);
  box-shadow:
    0 12px 24px rgba(15, 23, 42, 0.04),
    0 4px 10px rgba(59, 130, 246, 0.05);
  backdrop-filter: blur(10px);
}

.admin-permissions-page__filters-card :deep(.el-card__header),
.admin-permissions-page__table-card :deep(.el-card__header) {
  padding: 10px 14px 0;
  border-bottom: none;
}

.admin-permissions-page__filters-card :deep(.el-card__body) {
  padding: 8px 14px 12px;
}

.admin-permissions-page__table-card {
  display: flex;
  flex-direction: column;
  min-width: 0;
  min-height: 0;
  height: 100%;
}

.admin-permissions-page__table-card :deep(.el-card__body) {
  display: flex;
  flex: 1 1 auto;
  flex-direction: column;
  min-height: 0;
  padding: 8px 14px 12px;
}

.admin-permissions-page__filters-header,
.admin-permissions-page__table-header {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  align-items: center;
  padding-bottom: 4px;
}

.admin-permissions-page__card-title {
  color: #0f172a;
  font-size: 13px;
  font-weight: 700;
  line-height: 1.25;
}

.admin-permissions-page__card-meta {
  margin: 2px 0 0;
  color: #64748b;
  font-size: 11px;
  line-height: 1.4;
}

.admin-permissions-page__toolbar {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 10px;
  align-items: center;
}

.admin-permissions-page__fields {
  display: grid;
  grid-template-columns: minmax(240px, 1fr) minmax(150px, 180px);
  gap: 10px;
  align-items: center;
}

.admin-permissions-page__field {
  width: 100%;
}

.admin-permissions-page__field :deep(.el-input__wrapper),
.admin-permissions-page__field :deep(.el-select__wrapper) {
  min-height: 38px;
  padding: 0 10px;
  border-radius: 10px;
  background: rgba(248, 250, 252, 0.94);
  box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.18);
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.admin-permissions-page__field :deep(.el-input__inner),
.admin-permissions-page__field :deep(.el-select__placeholder),
.admin-permissions-page__field :deep(.el-select__selected-item) {
  color: #0f172a;
}

.admin-permissions-page__field :deep(.el-input__inner::placeholder) {
  color: #94a3b8;
}

.admin-permissions-page__field :deep(.el-input__wrapper:hover),
.admin-permissions-page__field :deep(.el-select__wrapper:hover) {
  background: #ffffff;
  box-shadow: inset 0 0 0 1px rgba(100, 116, 139, 0.28);
}

.admin-permissions-page__field :deep(.el-input__wrapper.is-focus),
.admin-permissions-page__field :deep(.el-select__wrapper.is-focused) {
  background: #ffffff;
  box-shadow:
    inset 0 0 0 1px rgba(59, 130, 246, 0.6),
    0 0 0 3px rgba(59, 130, 246, 0.1);
}

.admin-permissions-page__toolbar-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  justify-content: flex-end;
  flex-wrap: wrap;
}

.admin-permissions-page__button {
  min-height: 38px;
  padding: 0 14px;
  border-radius: 10px;
  font-weight: 600;
  transition:
    border-color 0.2s ease,
    background-color 0.2s ease,
    color 0.2s ease,
    box-shadow 0.2s ease;
}

.admin-permissions-page__button--primary {
  border-color: transparent;
  background: linear-gradient(135deg, #1e40af 0%, #2563eb 100%);
  box-shadow: 0 8px 18px rgba(37, 99, 235, 0.16);
}

.admin-permissions-page__button--ghost {
  border-color: rgba(148, 163, 184, 0.24);
  background: rgba(248, 250, 252, 0.92);
  color: #334155;
}

.admin-permissions-page__skeleton-shell {
  display: flex;
  flex: 1 1 auto;
  flex-direction: column;
  min-height: 0;
}

.admin-permissions-page__skeleton {
  display: grid;
  gap: 8px;
}

.admin-permissions-page__skeleton-row {
  width: 100%;
  height: 42px;
  border-radius: 12px;
}

.admin-permissions-page__table-shell {
  flex: 1 1 auto;
  min-width: 0;
  min-height: 0;
  overflow: hidden;
  padding: 3px;
  border: 1px solid rgba(148, 163, 184, 0.16);
  border-radius: 14px;
  background:
    linear-gradient(180deg, rgba(248, 250, 252, 0.92), rgba(255, 255, 255, 0.97));
}

.admin-permissions-page__table-shell :deep(.el-table) {
  min-width: 0;
  width: 100%;
  height: 100%;
  border-radius: 12px;
  color: #0f172a;
  --el-table-border-color: rgba(226, 232, 240, 0.95);
  --el-table-header-bg-color: rgba(241, 245, 249, 0.96);
  --el-table-row-hover-bg-color: rgba(239, 246, 255, 0.92);
  --el-table-tr-bg-color: rgba(255, 255, 255, 0.96);
}

.admin-permissions-page__table-shell :deep(.el-scrollbar) {
  height: 100%;
}

.admin-permissions-page__table-shell :deep(.el-scrollbar__wrap) {
  overflow-x: auto;
  overflow-y: auto;
}

.admin-permissions-page__table-shell :deep(.el-table__inner-wrapper) {
  min-width: 1120px;
}

.admin-permissions-page__table-shell :deep(.el-table th.el-table__cell) {
  padding: 8px 0;
  background: rgba(241, 245, 249, 0.96);
}

.admin-permissions-page__table-shell :deep(.el-table th.el-table__cell > .cell) {
  color: #334155;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
}

.admin-permissions-page__table-shell :deep(.el-table .el-table__cell) {
  padding: 8px 0;
}

.admin-permissions-page__table-shell :deep(.el-table .cell) {
  color: #0f172a;
  line-height: 1.45;
}

.admin-permissions-page__table-shell :deep(.el-table__row--striped td.el-table__cell) {
  background: rgba(248, 250, 252, 0.82);
}

.admin-permissions-page__table-shell :deep(.el-table__body-wrapper) {
  overflow-y: auto;
}

.admin-permissions-page__table-shell :deep(.el-tag) {
  min-height: 26px;
  padding: 0 10px;
  border-radius: 999px;
  font-weight: 600;
}

.admin-permissions-page__table-shell :deep(.el-button.is-link) {
  font-weight: 600;
}

.admin-permissions-page__table-shell :deep(.el-table__fixed-right::before),
.admin-permissions-page__table-shell :deep(.el-table__fixed::before) {
  background-color: rgba(226, 232, 240, 0.95);
}

.admin-permissions-page__row-actions {
  display: inline-flex;
  align-items: center;
  gap: 6px 8px;
  flex-wrap: wrap;
}

.admin-permissions-page__empty {
  display: grid;
  flex: 1 1 auto;
  min-height: 0;
  place-items: center;
  padding: 12px;
  border: 1px dashed rgba(148, 163, 184, 0.28);
  border-radius: 14px;
  background: linear-gradient(180deg, rgba(248, 250, 252, 0.72), rgba(255, 255, 255, 0.94));
}

.admin-permissions-page__empty :deep(.el-empty__description p) {
  color: #64748b;
}

@media (max-width: 920px) {
  .admin-permissions-page {
    grid-template-rows: auto auto auto;
    gap: 12px;
    height: auto;
    overflow: visible;
  }

  .admin-permissions-page__hero {
    padding-bottom: 6px;
  }

  .admin-permissions-page__filters-card :deep(.el-card__header),
  .admin-permissions-page__table-card :deep(.el-card__header) {
    padding: 12px 14px 0;
  }

  .admin-permissions-page__filters-card :deep(.el-card__body),
  .admin-permissions-page__table-card :deep(.el-card__body) {
    padding: 10px 14px 14px;
  }

  .admin-permissions-page__toolbar {
    grid-template-columns: 1fr;
    align-items: stretch;
  }

  .admin-permissions-page__fields {
    grid-template-columns: 1fr;
  }

  .admin-permissions-page__toolbar-actions {
    justify-content: flex-start;
  }

  .admin-permissions-page__table-card {
    height: auto;
  }

  .admin-permissions-page__table-shell {
    flex: none;
    min-height: 0;
    overflow-y: auto;
  }
}

@media (max-width: 640px) {
  .admin-permissions-page {
    gap: 10px;
  }

  .admin-permissions-page__title {
    font-size: clamp(20px, 7vw, 24px);
  }

  .admin-permissions-page__filters-card :deep(.el-card__header),
  .admin-permissions-page__table-card :deep(.el-card__header) {
    padding: 10px 12px 0;
  }

  .admin-permissions-page__filters-card :deep(.el-card__body),
  .admin-permissions-page__table-card :deep(.el-card__body) {
    padding: 8px 12px 12px;
  }

  .admin-permissions-page__button {
    min-width: 0;
    flex: 1 1 120px;
  }
}
</style>
