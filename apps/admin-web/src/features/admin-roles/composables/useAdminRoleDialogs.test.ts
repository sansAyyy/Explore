import { describe, expect, it, vi } from 'vitest'

import { useAdminRoleDialogs } from '@/features/admin-roles/composables/useAdminRoleDialogs'

const sampleRole = {
  id: 'role-1',
  code: 'super_admin',
  name: '超级管理员',
  description: '系统超级管理员',
  isActive: true,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: null,
}

describe('useAdminRoleDialogs', () => {
  it('coordinates refresh callbacks for create and update actions', async () => {
    const refreshRoles = vi.fn().mockResolvedValue(undefined)
    const dialogs = useAdminRoleDialogs({
      refreshRoles,
    })

    await dialogs.handleCreated()
    await dialogs.handleUpdated()

    expect(refreshRoles).toHaveBeenCalledTimes(2)
  })

  it('opens drawers with the selected role and clears permission assignment context', () => {
    const dialogs = useAdminRoleDialogs({
      refreshRoles: vi.fn(),
    })

    dialogs.handleOpenEditDrawer(sampleRole)
    dialogs.handleOpenAssignPermissionsDrawer(sampleRole)

    expect(dialogs.editingRole.value).toEqual(sampleRole)
    expect(dialogs.permissionAssigningRole.value).toEqual(sampleRole)

    dialogs.handlePermissionsChanged()

    expect(dialogs.editingRole.value).toEqual(sampleRole)
    expect(dialogs.permissionAssigningRole.value).toBeNull()
  })
})
