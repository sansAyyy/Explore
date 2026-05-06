import { describe, expect, it } from 'vitest'

import {
  buildAssignAdminUserRolesPayload,
  buildSelectableAdminRolesQuery,
  createDefaultAdminUserRoleSearchFilters,
  mapSelectableAdminRolesToOptions,
  resolveSelectedRoleIdsFromAssignments,
} from '@/features/admin-users/models/adminUserRoleAssignment'

describe('admin user role assignment model', () => {
  it('creates default search filters', () => {
    expect(createDefaultAdminUserRoleSearchFilters()).toEqual({
      keyword: '',
    })
  })

  it('builds selectable roles query with trimmed keyword', () => {
    expect(
      buildSelectableAdminRolesQuery({
        keyword: '  super  ',
      }),
    ).toEqual({
      pageIndex: 1,
      pageSize: 100,
      isActive: true,
      keyword: 'super',
    })
  })

  it('builds assign roles payload from selected role ids', () => {
    expect(buildAssignAdminUserRolesPayload(['role-1', 'role-2'])).toEqual({
      roleIds: ['role-1', 'role-2'],
    })
  })

  it('maps selectable roles to checkbox options', () => {
    expect(
      mapSelectableAdminRolesToOptions([
        {
          id: 'role-1',
          code: 'super_admin',
          name: '超级管理员',
          description: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: null,
        },
      ]),
    ).toEqual([
      {
        value: 'role-1',
        label: '超级管理员',
        code: 'super_admin',
      },
    ])
  })

  it('resolves only active assigned role ids', () => {
    expect(
      resolveSelectedRoleIdsFromAssignments([
        {
          id: 'role-1',
          code: 'super_admin',
          name: '超级管理员',
          isActive: true,
        },
        {
          id: 'role-2',
          code: 'legacy_admin',
          name: '历史角色',
          isActive: false,
        },
      ]),
    ).toEqual(['role-1'])
  })
})
