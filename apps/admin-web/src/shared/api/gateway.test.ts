import { describe, expect, it } from 'vitest'

import { buildServiceApiPath, gatewayServices } from '@/shared/api/gateway'

describe('buildServiceApiPath', () => {
  it('prefixes api paths with the selected service name', () => {
    expect(buildServiceApiPath(gatewayServices.adminIdentity, '/api/admin-users')).toBe(
      '/admin-identity/api/admin-users',
    )
  })

  it('normalizes paths without a leading slash', () => {
    expect(buildServiceApiPath(gatewayServices.adminIdentity, 'api/admin-users')).toBe(
      '/admin-identity/api/admin-users',
    )
  })

  it('does not produce duplicate slashes', () => {
    expect(buildServiceApiPath(gatewayServices.messageCenter, '//api//messages')).toBe(
      '/message-center/api/messages',
    )
  })

  it('supports the customer account gateway service', () => {
    expect(buildServiceApiPath(gatewayServices.customerAccount, '/api/admin-customers')).toBe(
      '/customer-account/api/admin-customers',
    )
  })
})
