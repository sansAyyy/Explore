import { describe, expect, it } from 'vitest'

import {
  buildChangeAdminUserPasswordPayload,
  createDefaultAdminUserChangePasswordForm,
  isAdminUserPasswordConfirmationMatched,
} from '@/features/admin-users/models/adminUserPasswordForm'

describe('admin user password form model', () => {
  it('creates default password form values', () => {
    expect(createDefaultAdminUserChangePasswordForm()).toEqual({
      newPassword: '',
      confirmPassword: '',
    })
  })

  it('trims password payload', () => {
    expect(
      buildChangeAdminUserPasswordPayload({
        newPassword: '  new-password  ',
        confirmPassword: '  ignored  ',
      }),
    ).toEqual({
      newPassword: 'new-password',
    })
  })

  it('checks password confirmation after trimming', () => {
    expect(
      isAdminUserPasswordConfirmationMatched({
        newPassword: '  same-password  ',
        confirmPassword: 'same-password',
      }),
    ).toBe(true)

    expect(
      isAdminUserPasswordConfirmationMatched({
        newPassword: 'password-a',
        confirmPassword: 'password-b',
      }),
    ).toBe(false)
  })
})
