import { beforeEach, describe, expect, it, vi } from 'vitest'

import { useAdminRoleActivation } from '@/features/admin-roles/composables/useAdminRoleActivation'

const adminRolesApi = vi.hoisted(() => ({
  disableAdminRole: vi.fn(),
  enableAdminRole: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
  success: vi.fn(),
}))

vi.mock('@/features/admin-roles/api/adminRolesApi', () => adminRolesApi)
vi.mock('element-plus', () => ({
  ElMessage: message,
}))

describe('useAdminRoleActivation', () => {
  beforeEach(() => {
    adminRolesApi.disableAdminRole.mockReset()
    adminRolesApi.enableAdminRole.mockReset()
    message.error.mockReset()
    message.success.mockReset()
  })

  it('enables a role and refreshes the list', async () => {
    adminRolesApi.enableAdminRole.mockResolvedValue(undefined)
    const refreshRoles = vi.fn().mockResolvedValue(undefined)
    const activation = useAdminRoleActivation({
      refreshRoles,
    })

    const row = {
      id: 'role-2',
      code: 'audit_reader',
      name: '审计只读',
      description: '只读审计角色',
      isActive: false,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: null,
    }

    const promise = activation.handleEnable(row)
    expect(activation.isRowActivationPending(row.id, 'enable')).toBe(true)
    await promise

    expect(adminRolesApi.enableAdminRole).toHaveBeenCalledWith('role-2')
    expect(refreshRoles).toHaveBeenCalledTimes(1)
    expect(message.success).toHaveBeenCalledWith('启用角色成功')
    expect(activation.isRowActivationPending(row.id)).toBe(false)
  })

  it('derives row-level activation availability from role state', () => {
    const activation = useAdminRoleActivation({
      refreshRoles: vi.fn(),
    })

    expect(
      activation.canEnableRow({
        id: 'role-2',
        code: 'audit_reader',
        name: '审计只读',
        description: null,
        isActive: false,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
      }),
    ).toBe(true)

    expect(
      activation.canDisableRow({
        id: 'role-2',
        code: 'audit_reader',
        name: '审计只读',
        description: null,
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
      }),
    ).toBe(true)
  })

  it('shows an error and clears pending state when activation fails', async () => {
    adminRolesApi.disableAdminRole.mockRejectedValue(new Error('request failed'))
    const refreshRoles = vi.fn()
    const activation = useAdminRoleActivation({
      refreshRoles,
    })

    const row = {
      id: 'role-3',
      code: 'legacy_role',
      name: '历史角色',
      description: null,
      isActive: true,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: null,
    }

    await activation.handleDisable(row)

    expect(refreshRoles).not.toHaveBeenCalled()
    expect(message.error).toHaveBeenCalledWith('request failed')
    expect(activation.isRowActivationPending(row.id)).toBe(false)
  })
})
