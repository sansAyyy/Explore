import { describe, expect, it } from 'vitest'

import {
  buildCustomerAccountsQuery,
  canDisableCustomerAccount,
  canEnableCustomerAccount,
  createDefaultCustomerAccountsFilters,
  createDefaultCustomerAccountsPagination,
  customerAccountsStatusOptions,
  formatCustomerAccountsStatus,
} from '@/features/customer-accounts/models/customerAccountsList'

describe('customer accounts list model', () => {
  it('exposes status options and default state', () => {
    expect(customerAccountsStatusOptions).toEqual([
      { label: '全部状态', value: 'all' },
      { label: '启用', value: 'enabled' },
      { label: '禁用', value: 'disabled' },
    ])

    expect(createDefaultCustomerAccountsFilters()).toEqual({
      keyword: '',
      status: 'all',
    })

    expect(createDefaultCustomerAccountsPagination()).toEqual({
      pageIndex: 1,
      pageSize: 10,
    })
  })

  it('builds query from filters and pagination', () => {
    expect(
      buildCustomerAccountsQuery(
        {
          keyword: '  13800138000  ',
          status: 'enabled',
        },
        {
          pageIndex: 2,
          pageSize: 20,
        },
      ),
    ).toEqual({
      pageIndex: 2,
      pageSize: 20,
      keyword: '13800138000',
      isActive: true,
    })
  })

  it('formats status values', () => {
    expect(formatCustomerAccountsStatus(true)).toBe('启用')
    expect(formatCustomerAccountsStatus(false)).toBe('禁用')
  })

  it('resolves activation actions for rows', () => {
    expect(canEnableCustomerAccount({ isActive: false })).toBe(true)
    expect(canEnableCustomerAccount({ isActive: true })).toBe(false)
    expect(canDisableCustomerAccount({ isActive: true })).toBe(true)
    expect(canDisableCustomerAccount({ isActive: false })).toBe(false)
  })
})
