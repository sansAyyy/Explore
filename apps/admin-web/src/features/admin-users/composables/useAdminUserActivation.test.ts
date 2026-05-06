import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'

import { useAdminUserActivation } from '@/features/admin-users/composables/useAdminUserActivation'

const adminUsersApi = vi.hoisted(() => ({
  disableAdminUser: vi.fn(),
  enableAdminUser: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
  success: vi.fn(),
}))

vi.mock('@/features/admin-users/api/adminUsersApi', () => adminUsersApi)
vi.mock('element-plus', () => ({
  ElMessage: message,
}))

describe('useAdminUserActivation', () => {
  beforeEach(() => {
    adminUsersApi.disableAdminUser.mockReset()
    adminUsersApi.enableAdminUser.mockReset()
    message.error.mockReset()
    message.success.mockReset()
  })

  it('enables a user and refreshes the list', async () => {
    adminUsersApi.enableAdminUser.mockResolvedValue(undefined)
    const refreshUsers = vi.fn().mockResolvedValue(undefined)
    const activation = useAdminUserActivation({
      currentUserId: ref('current-user'),
      refreshUsers,
    })

    const row = {
      id: 'user-2',
      userName: 'editor',
      email: 'editor@explore.local',
      phoneNumber: null,
      displayName: 'Editor',
      isActive: false,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: null,
      lastLoginAt: null,
    }

    const promise = activation.handleEnable(row)
    expect(activation.isRowActivationPending(row.id, 'enable')).toBe(true)
    await promise

    expect(adminUsersApi.enableAdminUser).toHaveBeenCalledWith('user-2')
    expect(refreshUsers).toHaveBeenCalledTimes(1)
    expect(message.success).toHaveBeenCalledWith('启用管理员成功')
    expect(activation.isRowActivationPending(row.id)).toBe(false)
  })

  it('derives row-level activation availability from user state', () => {
    const activation = useAdminUserActivation({
      currentUserId: ref('user-1'),
      refreshUsers: vi.fn(),
    })

    expect(
      activation.canEnableRow({
        id: 'user-2',
        userName: 'user-2',
        email: 'user-2@explore.local',
        phoneNumber: null,
        displayName: 'User 2',
        isActive: false,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
        lastLoginAt: null,
      }),
    ).toBe(true)

    expect(
      activation.canDisableRow({
        id: 'user-2',
        userName: 'user-2',
        email: 'user-2@explore.local',
        phoneNumber: null,
        displayName: 'User 2',
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
        lastLoginAt: null,
      }),
    ).toBe(true)

    expect(
      activation.canDisableRow({
        id: 'user-1',
        userName: 'current',
        email: 'current@explore.local',
        phoneNumber: null,
        displayName: 'Current User',
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
        lastLoginAt: null,
      }),
    ).toBe(false)
  })

  it('shows an error and clears pending state when activation fails', async () => {
    adminUsersApi.disableAdminUser.mockRejectedValue(new Error('request failed'))
    const refreshUsers = vi.fn()
    const activation = useAdminUserActivation({
      currentUserId: ref('current-user'),
      refreshUsers,
    })

    const row = {
      id: 'user-3',
      userName: 'legacy',
      email: 'legacy@explore.local',
      phoneNumber: null,
      displayName: 'Legacy User',
      isActive: true,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: null,
      lastLoginAt: null,
    }

    await activation.handleDisable(row)

    expect(refreshUsers).not.toHaveBeenCalled()
    expect(message.error).toHaveBeenCalledWith('request failed')
    expect(activation.isRowActivationPending(row.id)).toBe(false)
  })
})
