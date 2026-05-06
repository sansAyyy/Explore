<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { storeToRefs } from 'pinia'

import AdminUserAssignRolesDrawer from '@/features/admin-users/components/drawers/AdminUserAssignRolesDrawer.vue'
import AdminUserChangePasswordDrawer from '@/features/admin-users/components/drawers/AdminUserChangePasswordDrawer.vue'
import AdminUserCreateDrawer from '@/features/admin-users/components/drawers/AdminUserCreateDrawer.vue'
import AdminUserEditDrawer from '@/features/admin-users/components/drawers/AdminUserEditDrawer.vue'
import AdminUsersFiltersCard from '@/features/admin-users/components/list/AdminUsersFiltersCard.vue'
import AdminUsersTable from '@/features/admin-users/components/list/AdminUsersTable.vue'
import { useAdminUserActivation } from '@/features/admin-users/composables/useAdminUserActivation'
import { useAdminUserDialogs } from '@/features/admin-users/composables/useAdminUserDialogs'
import { useAdminUsersList } from '@/features/admin-users/composables/useAdminUsersList'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const { userId } = storeToRefs(authStore)
const list = useAdminUsersList()
const dialogs = useAdminUserDialogs({
  refreshUsers: list.loadUsers,
})
const activation = useAdminUserActivation({
  currentUserId: userId,
  refreshUsers: list.loadUsers,
})

const canCreateAdminUser = computed(() => authStore.hasButtonPermission('admin_users.create'))
const canUpdateAdminUser = computed(() => authStore.hasButtonPermission('admin_users.update'))
const canChangeAdminUserPassword = computed(() =>
  authStore.hasButtonPermission('admin_users.change_password'),
)
const canAssignAdminUserRoles = computed(() =>
  authStore.hasButtonPermission('admin_users.assign_roles'),
)

onMounted(() => {
  void list.loadUsers()
})
</script>

<template>
  <div class="admin-users-page">
    <section class="admin-users-page__hero">
      <p class="admin-users-page__eyebrow">User Admin</p>
      <h1 class="admin-users-page__title">用户管理</h1>
    </section>

    <section class="admin-users-page__section">
      <AdminUsersFiltersCard
        :can-create-admin-user="canCreateAdminUser"
        :filters="list.filters"
        :status-options="list.statusOptions"
        @create="dialogs.handleOpenCreateDrawer"
        @reset="list.handleReset"
        @search="list.handleSearch"
      />
    </section>

    <section class="admin-users-page__section admin-users-page__section--table">
      <AdminUsersTable
        :can-assign-admin-user-roles="canAssignAdminUserRoles"
        :can-change-admin-user-password="canChangeAdminUserPassword"
        :can-disable-row="activation.canDisableRow"
        :can-enable-row="activation.canEnableRow"
        :can-update-admin-user="canUpdateAdminUser"
        :current-user-id="authStore.userId"
        :is-loading="list.isLoading.value"
        :is-row-activation-pending="activation.isRowActivationPending"
        :pagination="list.pagination"
        :total-count="list.totalCount.value"
        :users="list.users.value"
        @assign-roles="dialogs.handleOpenAssignRolesDrawer"
        @change-password="dialogs.handleOpenChangePasswordDrawer"
        @disable="activation.handleDisable"
        @edit="dialogs.handleOpenEditDrawer"
        @enable="activation.handleEnable"
        @page-change="list.handlePageChange"
        @page-size-change="list.handlePageSizeChange"
      />
    </section>

    <AdminUserCreateDrawer
      v-model="dialogs.isCreateDrawerOpen.value"
      @created="dialogs.handleCreated"
    />
    <AdminUserEditDrawer
      v-model="dialogs.isEditDrawerOpen.value"
      :user="dialogs.editingUser.value"
      @updated="dialogs.handleUpdated"
    />
    <AdminUserChangePasswordDrawer
      v-model="dialogs.isChangePasswordDrawerOpen.value"
      :user="dialogs.passwordChangingUser.value"
      @changed="dialogs.handlePasswordChanged"
    />
    <AdminUserAssignRolesDrawer
      v-model="dialogs.isAssignRolesDrawerOpen.value"
      :user="dialogs.roleAssigningUser.value"
      @changed="dialogs.handleRolesChanged"
    />
  </div>
</template>

<style scoped>
.admin-users-page {
  display: grid;
  grid-template-rows: auto auto minmax(0, 1fr);
  gap: 10px;
  min-height: 0;
  height: 100%;
  color: #334155;
  overflow: hidden;
}

.admin-users-page__hero {
  display: grid;
  gap: 2px;
  min-width: 0;
  padding: 0 2px 4px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.9);
}

.admin-users-page__eyebrow {
  margin: 0;
  color: #64748b;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.admin-users-page__title {
  margin: 0;
  color: #0f172a;
  font-size: clamp(22px, 2vw, 26px);
  font-weight: 750;
  line-height: 1.04;
  letter-spacing: -0.03em;
}

.admin-users-page__section {
  min-width: 0;
}

.admin-users-page__section--table {
  min-height: 0;
}

@media (max-width: 920px) {
  .admin-users-page {
    grid-template-rows: auto auto auto;
    gap: 12px;
    height: auto;
    overflow: visible;
  }

  .admin-users-page__hero {
    padding-bottom: 6px;
  }
}

@media (max-width: 640px) {
  .admin-users-page {
    gap: 10px;
  }

  .admin-users-page__title {
    font-size: clamp(20px, 7vw, 24px);
  }
}
</style>
