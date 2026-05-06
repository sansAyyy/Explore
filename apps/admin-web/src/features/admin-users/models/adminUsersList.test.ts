import { describe, expect, it } from 'vitest'

import {
  adminUsersStatusOptions,
  buildAdminUsersQuery,
  canDisableAdminUser,
  canEnableAdminUser,
  createDefaultAdminUsersFilters,
  createDefaultAdminUsersPagination,
  formatAdminUsersStatus,
} from '@/features/admin-users/models/adminUsersList'

describe('admin users list model', () => {
  it('exposes status options and default state', () => {
    expect(adminUsersStatusOptions).toEqual([
      { label: '全部状态', value: 'all' },
      { label: '启用', value: 'enabled' },
      { label: '禁用', value: 'disabled' },
    ])

    expect(createDefaultAdminUsersFilters()).toEqual({
      keyword: '',
      status: 'all',
    })

    expect(createDefaultAdminUsersPagination()).toEqual({
      pageIndex: 1,
      pageSize: 10,
    })
  })

  it('builds query from filters and pagination', () => {
    expect(
      buildAdminUsersQuery(
        {
          keyword: '  seed-admin  ',
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
      keyword: 'seed-admin',
      isActive: true,
    })
  })

  it('formats status values', () => {
    expect(formatAdminUsersStatus(true)).toBe('启用')
    expect(formatAdminUsersStatus(false)).toBe('禁用')
  })

  it('resolves activation actions for rows', () => {
    expect(canEnableAdminUser({ isActive: false })).toBe(true)
    expect(canEnableAdminUser({ isActive: true })).toBe(false)
    expect(canDisableAdminUser({ id: '1', isActive: true }, '2')).toBe(true)
    expect(canDisableAdminUser({ id: '1', isActive: true }, '1')).toBe(false)
    expect(canDisableAdminUser({ id: '1', isActive: false }, '2')).toBe(false)
  })
})
