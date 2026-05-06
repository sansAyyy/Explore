import { describe, expect, it } from 'vitest'

import {
  buildCurrentAdminPasswordPayload,
  createDefaultCurrentAdminPasswordForm,
  isCurrentAdminPasswordChanged,
  isCurrentAdminPasswordConfirmationMatched,
} from '@/features/current-admin/models/currentAdminPasswordForm'

describe('current admin password form model', () => {
  it('creates default form values', () => {
    expect(createDefaultCurrentAdminPasswordForm()).toEqual({
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    })
  })

  it('builds the password payload from trimmed values', () => {
    expect(
      buildCurrentAdminPasswordPayload({
        currentPassword: '  current-pass  ',
        newPassword: '  next-pass  ',
        confirmPassword: ' next-pass ',
      }),
    ).toEqual({
      currentPassword: 'current-pass',
      newPassword: 'next-pass',
    })
  })

  it('checks password confirmation and password changes using trimmed values', () => {
    expect(
      isCurrentAdminPasswordConfirmationMatched({
        newPassword: '  Profile@123  ',
        confirmPassword: 'Profile@123',
      }),
    ).toBe(true)

    expect(
      isCurrentAdminPasswordChanged({
        currentPassword: 'ExamplePass@123',
        newPassword: 'ExamplePass@123',
      }),
    ).toBe(false)
  })
})
