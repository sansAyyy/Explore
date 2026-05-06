import { describe, expect, it } from 'vitest'

import {
  buildAdminRolesQuery,
  canDisableAdminRole,
  canEnableAdminRole,
  createDefaultAdminRolesFilters,
  createDefaultAdminRolesPagination,
  formatAdminRolesStatus,
  resolveAdminRolesStatusType,
} from '@/features/admin-roles/models/adminRolesList'

describe('admin roles list model', () => {
  it('creates default filters and pagination', () => {
    expect(createDefaultAdminRolesFilters()).toEqual({
      keyword: '',
      status: 'all',
    })

    expect(createDefaultAdminRolesPagination()).toEqual({
      pageIndex: 1,
      pageSize: 10,
    })
  })

  it('builds paged query with trimmed keyword and status mapping', () => {
    expect(
      buildAdminRolesQuery(
        {
          keyword: '  super  ',
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
      keyword: 'super',
      isActive: true,
    })
  })

  it('formats status and derives row action availability', () => {
    expect(formatAdminRolesStatus(true)).toBe('启用')
    expect(formatAdminRolesStatus(false)).toBe('禁用')
    expect(resolveAdminRolesStatusType(true)).toBe('success')
    expect(resolveAdminRolesStatusType(false)).toBe('info')
    expect(canEnableAdminRole({ isActive: false })).toBe(true)
    expect(canDisableAdminRole({ isActive: true })).toBe(true)
  })
})
