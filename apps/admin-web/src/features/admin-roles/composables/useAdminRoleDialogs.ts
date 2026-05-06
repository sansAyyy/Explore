import { ref, watch } from 'vue'

import type { AdminRoleBasicResponse } from '@/features/admin-roles/types/adminRoles'

export function useAdminRoleDialogs(options: {
  refreshRoles: () => Promise<unknown> | void
}) {
  const isCreateDrawerOpen = ref(false)
  const isEditDrawerOpen = ref(false)
  const isAssignPermissionsDrawerOpen = ref(false)
  const editingRole = ref<AdminRoleBasicResponse | null>(null)
  const permissionAssigningRole = ref<AdminRoleBasicResponse | null>(null)

  function handleOpenCreateDrawer() {
    isCreateDrawerOpen.value = true
  }

  function handleOpenEditDrawer(row: AdminRoleBasicResponse) {
    editingRole.value = row
    isEditDrawerOpen.value = true
  }

  function handleOpenAssignPermissionsDrawer(row: AdminRoleBasicResponse) {
    permissionAssigningRole.value = row
    isAssignPermissionsDrawerOpen.value = true
  }

  async function handleCreated() {
    await options.refreshRoles()
  }

  async function handleUpdated() {
    await options.refreshRoles()
  }

  function handlePermissionsChanged() {
    permissionAssigningRole.value = null
  }

  watch(isEditDrawerOpen, (value) => {
    if (!value) {
      editingRole.value = null
    }
  })

  watch(isAssignPermissionsDrawerOpen, (value) => {
    if (!value) {
      permissionAssigningRole.value = null
    }
  })

  return {
    editingRole,
    handleCreated,
    handleOpenAssignPermissionsDrawer,
    handleOpenCreateDrawer,
    handleOpenEditDrawer,
    handlePermissionsChanged,
    handleUpdated,
    isAssignPermissionsDrawerOpen,
    isCreateDrawerOpen,
    isEditDrawerOpen,
    permissionAssigningRole,
  }
}
