import { beforeEach, describe, expect, it, vi } from 'vitest'

import { useCustomerAccountActivation } from '@/features/customer-accounts/composables/useCustomerAccountActivation'

const customerAccountsApi = vi.hoisted(() => ({
  disableCustomerAccount: vi.fn(),
  enableCustomerAccount: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
  success: vi.fn(),
}))

vi.mock('@/features/customer-accounts/api/customerAccountsApi', () => customerAccountsApi)
vi.mock('element-plus', () => ({
  ElMessage: message,
}))

describe('useCustomerAccountActivation', () => {
  beforeEach(() => {
    customerAccountsApi.disableCustomerAccount.mockReset()
    customerAccountsApi.enableCustomerAccount.mockReset()
    message.error.mockReset()
    message.success.mockReset()
  })

  it('enables a customer and refreshes the list', async () => {
    customerAccountsApi.enableCustomerAccount.mockResolvedValue(undefined)
    const refreshCustomers = vi.fn().mockResolvedValue(undefined)
    const activation = useCustomerAccountActivation({
      refreshCustomers,
    })

    const row = {
      id: 'customer-2',
      phoneNumber: '13800138001',
      nickName: 'Bob',
      avatarUrl: null,
      email: null,
      isActive: false,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: null,
      lastLoginAt: null,
    }

    const promise = activation.handleEnable(row)
    expect(activation.isRowActivationPending(row.id, 'enable')).toBe(true)
    await promise

    expect(customerAccountsApi.enableCustomerAccount).toHaveBeenCalledWith('customer-2')
    expect(refreshCustomers).toHaveBeenCalledTimes(1)
    expect(message.success).toHaveBeenCalledWith('启用客户成功')
    expect(activation.isRowActivationPending(row.id)).toBe(false)
  })

  it('derives row-level activation availability from customer state', () => {
    const activation = useCustomerAccountActivation({
      refreshCustomers: vi.fn(),
    })

    expect(
      activation.canEnableRow({
        id: 'customer-2',
        phoneNumber: '13800138001',
        nickName: 'Bob',
        avatarUrl: null,
        email: null,
        isActive: false,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
        lastLoginAt: null,
      }),
    ).toBe(true)

    expect(
      activation.canDisableRow({
        id: 'customer-2',
        phoneNumber: '13800138001',
        nickName: 'Bob',
        avatarUrl: null,
        email: null,
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
        lastLoginAt: null,
      }),
    ).toBe(true)
  })

  it('shows an error and clears pending state when activation fails', async () => {
    customerAccountsApi.disableCustomerAccount.mockRejectedValue(new Error('request failed'))
    const refreshCustomers = vi.fn()
    const activation = useCustomerAccountActivation({
      refreshCustomers,
    })

    const row = {
      id: 'customer-3',
      phoneNumber: '13800138002',
      nickName: 'Legacy',
      avatarUrl: null,
      email: null,
      isActive: true,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: null,
      lastLoginAt: null,
    }

    await activation.handleDisable(row)

    expect(refreshCustomers).not.toHaveBeenCalled()
    expect(message.error).toHaveBeenCalledWith('request failed')
    expect(activation.isRowActivationPending(row.id)).toBe(false)
  })
})
