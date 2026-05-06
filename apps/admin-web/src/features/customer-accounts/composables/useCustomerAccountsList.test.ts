import { beforeEach, describe, expect, it, vi } from 'vitest'

import { useCustomerAccountsList } from '@/features/customer-accounts/composables/useCustomerAccountsList'

const customerAccountsApi = vi.hoisted(() => ({
  getPagedCustomerAccounts: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
}))

vi.mock('@/features/customer-accounts/api/customerAccountsApi', () => customerAccountsApi)
vi.mock('element-plus', () => ({
  ElMessage: message,
}))

describe('useCustomerAccountsList', () => {
  beforeEach(() => {
    customerAccountsApi.getPagedCustomerAccounts.mockReset()
    message.error.mockReset()
  })

  it('loads customers and updates result state', async () => {
    customerAccountsApi.getPagedCustomerAccounts.mockResolvedValue({
      totalCount: 1,
      items: [
        {
          id: 'customer-1',
          phoneNumber: '13800138000',
          nickName: 'Alice',
          avatarUrl: null,
          email: 'alice@explore.local',
          isActive: true,
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: null,
          lastLoginAt: null,
        },
      ],
    })

    const list = useCustomerAccountsList()
    await list.loadCustomers()

    expect(customerAccountsApi.getPagedCustomerAccounts).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 10,
    })
    expect(list.customers.value).toHaveLength(1)
    expect(list.totalCount.value).toBe(1)
    expect(list.isLoading.value).toBe(false)
  })

  it('resets page index and reloads with current filters on search', async () => {
    customerAccountsApi.getPagedCustomerAccounts.mockResolvedValue({
      totalCount: 0,
      items: [],
    })

    const list = useCustomerAccountsList()
    list.pagination.pageIndex = 3
    list.filters.keyword = '  alice '
    list.filters.status = 'enabled'

    await list.handleSearch()

    expect(list.pagination.pageIndex).toBe(1)
    expect(customerAccountsApi.getPagedCustomerAccounts).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 10,
      keyword: 'alice',
      isActive: true,
    })
  })

  it('restores default filters and pagination on reset', async () => {
    customerAccountsApi.getPagedCustomerAccounts.mockResolvedValue({
      totalCount: 0,
      items: [],
    })

    const list = useCustomerAccountsList()
    list.filters.keyword = 'legacy'
    list.filters.status = 'disabled'
    list.pagination.pageIndex = 4
    list.pagination.pageSize = 50

    await list.handleReset()

    expect(list.filters).toMatchObject({
      keyword: '',
      status: 'all',
    })
    expect(list.pagination).toMatchObject({
      pageIndex: 1,
      pageSize: 10,
    })
    expect(customerAccountsApi.getPagedCustomerAccounts).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 10,
    })
  })

  it('clears stale data and shows an error message when loading fails', async () => {
    customerAccountsApi.getPagedCustomerAccounts.mockRejectedValue(new Error('request failed'))

    const list = useCustomerAccountsList()
    list.customers.value = [
      {
        id: 'customer-1',
        phoneNumber: '13800138000',
        nickName: 'Stale Customer',
        avatarUrl: null,
        email: null,
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
        lastLoginAt: null,
      },
    ]
    list.totalCount.value = 1

    await list.loadCustomers()

    expect(list.customers.value).toEqual([])
    expect(list.totalCount.value).toBe(0)
    expect(message.error).toHaveBeenCalledWith('request failed')
  })
})
