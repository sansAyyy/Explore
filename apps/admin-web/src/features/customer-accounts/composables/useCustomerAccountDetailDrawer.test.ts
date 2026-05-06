import { beforeEach, describe, expect, it, vi } from 'vitest'
import { nextTick } from 'vue'

import { useCustomerAccountDetailDrawer } from '@/features/customer-accounts/composables/useCustomerAccountDetailDrawer'

const customerAccountsApi = vi.hoisted(() => ({
  getCustomerAccountById: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
}))

vi.mock('@/features/customer-accounts/api/customerAccountsApi', () => customerAccountsApi)
vi.mock('element-plus', () => ({
  ElMessage: message,
}))

describe('useCustomerAccountDetailDrawer', () => {
  beforeEach(() => {
    customerAccountsApi.getCustomerAccountById.mockReset()
    message.error.mockReset()
  })

  it('opens the drawer and loads detail for the selected customer', async () => {
    customerAccountsApi.getCustomerAccountById.mockResolvedValue({
      id: 'customer-1',
      phoneNumber: '13800138000',
      nickName: 'Alice',
      avatarUrl: null,
      email: 'alice@explore.local',
      isActive: true,
      createdAt: '2026-01-01T00:00:00Z',
      createdBy: 'migration',
      updatedAt: null,
      updatedBy: null,
      lastLoginAt: null,
      version: 1,
    })

    const detailDrawer = useCustomerAccountDetailDrawer()
    detailDrawer.handleOpenDetailDrawer({
      id: 'customer-1',
      phoneNumber: '13800138000',
      nickName: 'Alice',
      avatarUrl: null,
      email: 'alice@explore.local',
      isActive: true,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: null,
      lastLoginAt: null,
    })

    await nextTick()
    await Promise.resolve()

    expect(detailDrawer.isDetailDrawerOpen.value).toBe(true)
    expect(customerAccountsApi.getCustomerAccountById).toHaveBeenCalledWith('customer-1')
    expect(detailDrawer.detail.value).toEqual(
      expect.objectContaining({
        id: 'customer-1',
        nickName: 'Alice',
      }),
    )
  })

  it('closes the drawer and reports an error when detail loading fails', async () => {
    customerAccountsApi.getCustomerAccountById.mockRejectedValue(new Error('request failed'))

    const detailDrawer = useCustomerAccountDetailDrawer()
    detailDrawer.handleOpenDetailDrawer({
      id: 'customer-2',
      phoneNumber: '13800138001',
      nickName: 'Bob',
      avatarUrl: null,
      email: null,
      isActive: true,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: null,
      lastLoginAt: null,
    })

    await nextTick()
    await Promise.resolve()

    expect(message.error).toHaveBeenCalledWith('request failed')
    expect(detailDrawer.isDetailDrawerOpen.value).toBe(false)
    expect(detailDrawer.selectedCustomer.value).toBeNull()
    expect(detailDrawer.detail.value).toBeNull()
  })
})
