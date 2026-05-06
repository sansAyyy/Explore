import { describe, expect, it, vi } from 'vitest'

import { useAdminUserDialogs } from '@/features/admin-users/composables/useAdminUserDialogs'

const sampleUser = {
  id: 'user-1',
  userName: 'seed-admin',
  email: 'seed-admin@example.test',
  phoneNumber: null,
  displayName: 'Platform Admin',
  isActive: true,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: null,
  lastLoginAt: null,
}

describe('useAdminUserDialogs', () => {
  it('coordinates refresh callbacks for create and update actions', async () => {
    const refreshUsers = vi.fn().mockResolvedValue(undefined)
    const dialogs = useAdminUserDialogs({
      refreshUsers,
    })

    await dialogs.handleCreated()
    await dialogs.handleUpdated()

    expect(refreshUsers).toHaveBeenCalledTimes(2)
  })

  it('opens drawers with the selected user and clears contextual state on completion', () => {
    const dialogs = useAdminUserDialogs({
      refreshUsers: vi.fn(),
    })

    dialogs.handleOpenEditDrawer(sampleUser)
    dialogs.handleOpenChangePasswordDrawer(sampleUser)
    dialogs.handleOpenAssignRolesDrawer(sampleUser)

    expect(dialogs.editingUser.value).toEqual(sampleUser)
    expect(dialogs.passwordChangingUser.value).toEqual(sampleUser)
    expect(dialogs.roleAssigningUser.value).toEqual(sampleUser)

    dialogs.handlePasswordChanged()
    dialogs.handleRolesChanged()

    expect(dialogs.editingUser.value).toEqual(sampleUser)
    expect(dialogs.passwordChangingUser.value).toBeNull()
    expect(dialogs.roleAssigningUser.value).toBeNull()
  })
})
