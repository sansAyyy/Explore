import { ref, watch } from 'vue'

import type { AdminUserBasicResponse } from '@/features/admin-users/types/adminUsers'

export function useAdminUserDialogs(options: {
  refreshUsers: () => Promise<unknown> | void
}) {
  const isCreateDrawerOpen = ref(false)
  const isEditDrawerOpen = ref(false)
  const isChangePasswordDrawerOpen = ref(false)
  const isAssignRolesDrawerOpen = ref(false)
  const editingUser = ref<AdminUserBasicResponse | null>(null)
  const passwordChangingUser = ref<AdminUserBasicResponse | null>(null)
  const roleAssigningUser = ref<AdminUserBasicResponse | null>(null)

  function handleOpenCreateDrawer() {
    isCreateDrawerOpen.value = true
  }

  function handleOpenEditDrawer(row: AdminUserBasicResponse) {
    editingUser.value = row
    isEditDrawerOpen.value = true
  }

  function handleOpenChangePasswordDrawer(row: AdminUserBasicResponse) {
    passwordChangingUser.value = row
    isChangePasswordDrawerOpen.value = true
  }

  function handleOpenAssignRolesDrawer(row: AdminUserBasicResponse) {
    roleAssigningUser.value = row
    isAssignRolesDrawerOpen.value = true
  }

  async function handleCreated() {
    await options.refreshUsers()
  }

  async function handleUpdated() {
    await options.refreshUsers()
  }

  function handlePasswordChanged() {
    passwordChangingUser.value = null
  }

  function handleRolesChanged() {
    roleAssigningUser.value = null
  }

  watch(isEditDrawerOpen, (value) => {
    if (!value) {
      editingUser.value = null
    }
  })

  watch(isChangePasswordDrawerOpen, (value) => {
    if (!value) {
      passwordChangingUser.value = null
    }
  })

  watch(isAssignRolesDrawerOpen, (value) => {
    if (!value) {
      roleAssigningUser.value = null
    }
  })

  return {
    editingUser,
    handleCreated,
    handleOpenAssignRolesDrawer,
    handleOpenChangePasswordDrawer,
    handleOpenCreateDrawer,
    handleOpenEditDrawer,
    handlePasswordChanged,
    handleRolesChanged,
    handleUpdated,
    isAssignRolesDrawerOpen,
    isChangePasswordDrawerOpen,
    isCreateDrawerOpen,
    isEditDrawerOpen,
    passwordChangingUser,
    roleAssigningUser,
  }
}
