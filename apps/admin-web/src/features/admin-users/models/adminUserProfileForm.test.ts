import { describe, expect, it } from 'vitest'

import {
  buildAdminUserProfilePayload,
  buildCreateAdminUserPayload,
  createAdminUserProfileFormFromUser,
  createDefaultAdminUserCreateForm,
  createDefaultAdminUserProfileForm,
} from '@/features/admin-users/models/adminUserProfileForm'

describe('admin user profile form model', () => {
  it('creates default profile and create form values', () => {
    expect(createDefaultAdminUserProfileForm()).toEqual({
      userName: '',
      email: '',
      displayName: '',
      phoneNumber: '',
    })

    expect(createDefaultAdminUserCreateForm()).toEqual({
      userName: '',
      email: '',
      displayName: '',
      phoneNumber: '',
      password: '',
      isActive: true,
    })
  })

  it('trims profile fields and turns empty phone number into null', () => {
    expect(
      buildAdminUserProfilePayload({
        userName: '  seed-admin  ',
        email: '  seed-admin@example.test  ',
        displayName: '  Platform Admin  ',
        phoneNumber: '   ',
      }),
    ).toEqual({
      userName: 'seed-admin',
      email: 'seed-admin@example.test',
      displayName: 'Platform Admin',
      phoneNumber: null,
    })
  })

  it('builds create payload from shared profile fields', () => {
    expect(
      buildCreateAdminUserPayload({
        userName: '  seed-admin  ',
        email: '  seed-admin@example.test  ',
        displayName: '  Platform Admin  ',
        phoneNumber: ' 13800138000 ',
        password: '  password123  ',
        isActive: false,
      }),
    ).toEqual({
      userName: 'seed-admin',
      email: 'seed-admin@example.test',
      displayName: 'Platform Admin',
      phoneNumber: '13800138000',
      password: 'password123',
      isActive: false,
    })
  })

  it('maps a table row into editable profile form values', () => {
    expect(
      createAdminUserProfileFormFromUser({
        userName: 'seed-admin',
        email: 'seed-admin@example.test',
        displayName: 'Platform Admin',
        phoneNumber: null,
      }),
    ).toEqual({
      userName: 'seed-admin',
      email: 'seed-admin@example.test',
      displayName: 'Platform Admin',
      phoneNumber: '',
    })
  })
})
