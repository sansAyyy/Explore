import type { CustomerSession } from '@/features/customer-account/types/customerAccount'

const CUSTOMER_SESSION_STORAGE_KEY = 'explore.customer-miniapp.customer-session'

export function getPersistedCustomerSession(): CustomerSession | null {
  const rawValue = uni.getStorageSync(CUSTOMER_SESSION_STORAGE_KEY)

  if (!rawValue || typeof rawValue !== 'string') {
    return null
  }

  try {
    const parsed = JSON.parse(rawValue) as unknown
    return isCustomerSession(parsed) ? parsed : null
  } catch {
    return null
  }
}

export function persistCustomerSession(session: CustomerSession) {
  uni.setStorageSync(CUSTOMER_SESSION_STORAGE_KEY, JSON.stringify(session))
}

export function clearPersistedCustomerSession() {
  uni.removeStorageSync(CUSTOMER_SESSION_STORAGE_KEY)
}

function isCustomerSession(value: unknown): value is CustomerSession {
  if (!value || typeof value !== 'object' || Array.isArray(value)) {
    return false
  }

  const candidate = value as Record<string, unknown>

  return (
    typeof candidate.accessToken === 'string' &&
    typeof candidate.refreshToken === 'string' &&
    typeof candidate.tokenType === 'string' &&
    typeof candidate.expiresAt === 'string' &&
    typeof candidate.refreshTokenExpiresAt === 'string'
  )
}
