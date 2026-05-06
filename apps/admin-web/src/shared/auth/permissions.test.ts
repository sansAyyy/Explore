import { describe, expect, it } from 'vitest'

import { hasButtonPermission, hasPagePermission } from '@/shared/auth/permissions'

describe('permission helpers', () => {
  it('returns true when page permission is missing', () => {
    expect(hasPagePermission(['dashboard.view'], undefined)).toBe(true)
  })

  it('checks page permission from code list', () => {
    expect(hasPagePermission(['dashboard.view'], 'dashboard.view')).toBe(true)
    expect(hasPagePermission(['dashboard.view'], 'admin_users.page')).toBe(false)
  })

  it('checks button permission from code list', () => {
    expect(hasButtonPermission(['admin_users.create'], 'admin_users.create')).toBe(true)
    expect(hasButtonPermission(['admin_users.create'], 'admin_users.delete')).toBe(false)
  })
})
