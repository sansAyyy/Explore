<script setup lang="ts">
import { ElMessage } from 'element-plus'
import { computed, onMounted, reactive } from 'vue'
import { useRouter } from 'vue-router'

import { deleteAdminRole } from '@/features/admin-roles/api/adminRolesApi'
import AdminRoleCreateDrawer from '@/features/admin-roles/components/drawers/AdminRoleCreateDrawer.vue'
import AdminRoleEditDrawer from '@/features/admin-roles/components/drawers/AdminRoleEditDrawer.vue'
import AdminRolesFiltersCard from '@/features/admin-roles/components/list/AdminRolesFiltersCard.vue'
import AdminRolesTable from '@/features/admin-roles/components/list/AdminRolesTable.vue'
import { useAdminRoleActivation } from '@/features/admin-roles/composables/useAdminRoleActivation'
import { useAdminRoleDialogs } from '@/features/admin-roles/composables/useAdminRoleDialogs'
import { useAdminRolesList } from '@/features/admin-roles/composables/useAdminRolesList'
import type { AdminRoleBasicResponse } from '@/features/admin-roles/types/adminRoles'
import { extractErrorMessage } from '@/shared/api/error'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const router = useRouter()
const list = useAdminRolesList()
const dialogs = useAdminRoleDialogs({
  refreshRoles: list.loadRoles,
})
const activation = useAdminRoleActivation({
  refreshRoles: list.loadRoles,
})

const deleteState = reactive<{
  roleId: string | null
}>({
  roleId: null,
})

const canCreateAdminRole = computed(() => authStore.hasButtonPermission('admin_roles.create'))
const canUpdateAdminRole = computed(() => authStore.hasButtonPermission('admin_roles.update'))
const canDeleteAdminRole = computed(() => authStore.hasButtonPermission('admin_roles.delete'))
const canAssignAdminRolePermissions = computed(() =>
  authStore.hasButtonPermission('admin_roles.assign_permissions'),
)

function isRowDeletionPending(roleId: string) {
  return deleteState.roleId === roleId
}

function handleOpenPermissionsPage(row: AdminRoleBasicResponse) {
  void router.push({
    name: 'admin-role-permissions',
    params: {
      roleId: row.id,
    },
  })
}

async function handleDelete(row: AdminRoleBasicResponse) {
  deleteState.roleId = row.id

  try {
    await deleteAdminRole(row.id)
    ElMessage.success('删除角色成功')
    await list.loadRoles()
  } catch (error) {
    ElMessage.error(extractErrorMessage(error, '删除角色失败'))
  } finally {
    deleteState.roleId = null
  }
}

onMounted(() => {
  void list.loadRoles()
})
</script>

<template>
  <div class="admin-roles-page">
    <section class="admin-roles-page__hero">
      <p class="admin-roles-page__eyebrow">Role Admin</p>
      <h1 class="admin-roles-page__title">角色管理</h1>
    </section>

    <section class="admin-roles-page__section">
      <AdminRolesFiltersCard
        :can-create-admin-role="canCreateAdminRole"
        :filters="list.filters"
        :status-options="list.statusOptions"
        @create="dialogs.handleOpenCreateDrawer"
        @reset="list.handleReset"
        @search="list.handleSearch"
      />
    </section>

    <section class="admin-roles-page__section admin-roles-page__section--table">
      <AdminRolesTable
        :can-assign-admin-role-permissions="canAssignAdminRolePermissions"
        :can-delete-admin-role="canDeleteAdminRole"
        :can-disable-row="activation.canDisableRow"
        :can-enable-row="activation.canEnableRow"
        :can-update-admin-role="canUpdateAdminRole"
        :is-loading="list.isLoading.value"
        :is-row-activation-pending="activation.isRowActivationPending"
        :is-row-deletion-pending="isRowDeletionPending"
        :pagination="list.pagination"
        :roles="list.roles.value"
        :total-count="list.totalCount.value"
        @assign-permissions="handleOpenPermissionsPage"
        @delete="handleDelete"
        @disable="activation.handleDisable"
        @edit="dialogs.handleOpenEditDrawer"
        @enable="activation.handleEnable"
        @page-change="list.handlePageChange"
        @page-size-change="list.handlePageSizeChange"
      />
    </section>

    <AdminRoleCreateDrawer
      v-model="dialogs.isCreateDrawerOpen.value"
      @created="dialogs.handleCreated"
    />
    <AdminRoleEditDrawer
      v-model="dialogs.isEditDrawerOpen.value"
      :role="dialogs.editingRole.value"
      @updated="dialogs.handleUpdated"
    />
  </div>
</template>

<style scoped>
.admin-roles-page {
  display: grid;
  grid-template-rows: auto auto minmax(0, 1fr);
  gap: 10px;
  min-height: 0;
  height: 100%;
  color: #334155;
  overflow: hidden;
}

.admin-roles-page__hero {
  display: grid;
  gap: 2px;
  min-width: 0;
  padding: 0 2px 4px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.9);
}

.admin-roles-page__eyebrow {
  margin: 0;
  color: #64748b;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.admin-roles-page__title {
  margin: 0;
  color: #0f172a;
  font-size: clamp(22px, 2vw, 26px);
  font-weight: 750;
  line-height: 1.04;
  letter-spacing: -0.03em;
}

.admin-roles-page__section {
  min-width: 0;
}

.admin-roles-page__section--table {
  min-height: 0;
}

@media (max-width: 920px) {
  .admin-roles-page {
    grid-template-rows: auto auto auto;
    gap: 12px;
    height: auto;
    overflow: visible;
  }

  .admin-roles-page__hero {
    padding-bottom: 6px;
  }
}

@media (max-width: 640px) {
  .admin-roles-page {
    gap: 10px;
  }

  .admin-roles-page__title {
    font-size: clamp(20px, 7vw, 24px);
  }
}
</style>
