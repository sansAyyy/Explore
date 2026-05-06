export const gatewayServices = {
  adminIdentity: 'admin-identity',
  customerAccount: 'customer-account',
  messageCenter: 'message-center',
} as const

export type GatewayService = (typeof gatewayServices)[keyof typeof gatewayServices]

function normalizePath(path: string) {
  const withLeadingSlash = path.startsWith('/') ? path : `/${path}`
  return withLeadingSlash.replace(/\/{2,}/g, '/')
}

export function buildServiceApiPath(service: GatewayService, path: string) {
  return normalizePath(`/${service}${normalizePath(path)}`)
}
