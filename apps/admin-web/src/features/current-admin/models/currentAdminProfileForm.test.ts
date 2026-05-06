import { describe, expect, it } from 'vitest'

import {
  buildCurrentAdminProfilePayload,
  createCurrentAdminProfileFormFromResponse,
  createDefaultCurrentAdminProfileForm,
} from '@/features/current-admin/models/currentAdminProfileForm'

describe('current admin profile form model', () => {
  it('creates default form values', () => {
    expect(createDefaultCurrentAdminProfileForm()).toEqual({
      userName: '',
      email: '',
      displayName: '',
    })
  })

  it('maps current admin details into editable form values', () => {
    expect(
      createCurrentAdminProfileFormFromResponse({
        userName: 'ops_admin',
        email: 'ops@explore.local',
        displayName: 'Operations Admin',
      }),
    ).toEqual({
      userName: 'ops_admin',
      email: 'ops@explore.local',
      displayName: 'Operations Admin',
    })
  })

  it('trims editable fields before building the update payload', () => {
    expect(
      buildCurrentAdminProfilePayload({
        userName: '  profile-admin  ',
        email: '  profile@explore.local  ',
        displayName: '  Profile Admin  ',
      }),
    ).toEqual({
      userName: 'profile-admin',
      email: 'profile@explore.local',
      displayName: 'Profile Admin',
    })
  })
})
