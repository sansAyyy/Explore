import { describe, expect, it } from 'vitest'

import {
  buildAdminRoleProfilePayload,
  buildCreateAdminRolePayload,
  createAdminRoleProfileFormFromRole,
  createDefaultAdminRoleCreateForm,
  createDefaultAdminRoleProfileForm,
} from '@/features/admin-roles/models/adminRoleForm'

describe('admin role form model', () => {
  it('creates default profile and create form values', () => {
    expect(createDefaultAdminRoleProfileForm()).toEqual({
      code: '',
      name: '',
      description: '',
    })

    expect(createDefaultAdminRoleCreateForm()).toEqual({
      code: '',
      name: '',
      description: '',
      isActive: true,
    })
  })

  it('trims profile fields and turns empty description into null', () => {
    expect(
      buildAdminRoleProfilePayload({
        code: '  super_admin  ',
        name: '  超级管理员  ',
        description: '   ',
      }),
    ).toEqual({
      code: 'super_admin',
      name: '超级管理员',
      description: null,
    })
  })

  it('builds create payload from shared role fields and maps rows into form values', () => {
    expect(
      buildCreateAdminRolePayload({
        code: '  audit_reader  ',
        name: '  审计只读  ',
        description: '  只读审计角色  ',
        isActive: false,
      }),
    ).toEqual({
      code: 'audit_reader',
      name: '审计只读',
      description: '只读审计角色',
      isActive: false,
    })

    expect(
      createAdminRoleProfileFormFromRole({
        code: 'super_admin',
        name: '超级管理员',
        description: null,
      }),
    ).toEqual({
      code: 'super_admin',
      name: '超级管理员',
      description: '',
    })
  })
})
