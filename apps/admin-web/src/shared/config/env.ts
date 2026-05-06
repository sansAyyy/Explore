export function getGatewayBaseUrl() {
  return import.meta.env.VITE_GATEWAY_BASE_URL || 'http://localhost:5203'
}
