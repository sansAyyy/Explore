function trimTrailingSlash(value: string) {
  return value.replace(/\/+$/g, '')
}

export function getGatewayBaseUrl() {
  const configuredUrl = import.meta.env.VITE_GATEWAY_BASE_URL?.trim()

  if (!configuredUrl) {
    return 'http://localhost:5203'
  }

  return trimTrailingSlash(configuredUrl)
}
