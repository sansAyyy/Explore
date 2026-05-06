import { computed, reactive, readonly } from 'vue'

import { customerAuthApi } from '@/features/customer-account/api/customerAuthApi'
import { currentCustomerApi } from '@/features/customer-account/api/currentCustomerApi'
import {
  clearPersistedCustomerSession,
  getPersistedCustomerSession,
  persistCustomerSession,
} from '@/features/customer-account/storage/customerSessionStorage'
import type {
  CurrentCustomer,
  CustomerLoginPayload,
  CustomerLoginResponse,
  CustomerSession,
  UpdateCurrentCustomerAvatarPayload,
  UpdateCurrentCustomerProfilePayload,
} from '@/features/customer-account/types/customerAccount'
import { configureHttpAuth, isApiError } from '@/shared/api/http'

interface CustomerSessionState {
  session: CustomerSession | null
  currentCustomer: CurrentCustomer | null
  isRestoring: boolean
  isRefreshing: boolean
  isLoadingCustomer: boolean
  lastErrorMessage: string | null
}

const state = reactive<CustomerSessionState>({
  session: null,
  currentCustomer: null,
  isRestoring: false,
  isRefreshing: false,
  isLoadingCustomer: false,
  lastErrorMessage: null,
})

const isAuthenticated = computed(() => Boolean(state.session?.accessToken))

let restorePromise: Promise<CurrentCustomer | null> | null = null
let refreshPromise: Promise<boolean> | null = null
let currentCustomerPromise: Promise<CurrentCustomer | null> | null = null

configureHttpAuth({
  getAccessToken: () => state.session?.accessToken ?? null,
  hasRefreshToken: () => Boolean(state.session?.refreshToken),
  refreshSession,
  clearSession: () => {
    clearSession()
  },
})

export function useCustomerSession() {
  return {
    state: readonly(state),
    isAuthenticated,
    restoreSession,
    sendPhoneLoginCode,
    loginWithPhone,
    logout,
    fetchCurrentCustomer,
    updateCurrentCustomerProfile,
    updateCurrentCustomerAvatar,
    clearSession,
  }
}

async function restoreSession() {
  if (restorePromise) {
    return restorePromise
  }

  if (state.session) {
    if (state.currentCustomer || state.isLoadingCustomer) {
      return state.currentCustomer
    }

    return fetchCurrentCustomer()
  }

  state.isRestoring = true
  state.lastErrorMessage = null

  restorePromise = (async () => {
    try {
      const persistedSession = getPersistedCustomerSession()

      if (!persistedSession || isRefreshTokenExpired(persistedSession)) {
        clearSession()
        return null
      }

      applySession(persistedSession)
      return await fetchCurrentCustomer()
    } catch {
      return state.currentCustomer
    } finally {
      state.isRestoring = false
      restorePromise = null
    }
  })()

  return restorePromise
}

async function sendPhoneLoginCode(phoneNumber: string) {
  state.lastErrorMessage = null
  await customerAuthApi.sendPhoneLoginCode({ phoneNumber })
}

async function loginWithPhone(payload: CustomerLoginPayload) {
  state.lastErrorMessage = null

  const response = await customerAuthApi.phoneLogin(payload)
  applySession(mapCustomerSession(response))
  await fetchCurrentCustomer(true)

  return state.currentCustomer
}

async function refreshSession() {
  if (refreshPromise) {
    return refreshPromise
  }

  const refreshToken = state.session?.refreshToken ?? getPersistedCustomerSession()?.refreshToken

  if (!refreshToken) {
    clearSession()
    return false
  }

  refreshPromise = (async () => {
    state.isRefreshing = true

    try {
      const response = await customerAuthApi.refresh({ refreshToken })
      applySession(mapCustomerSession(response))
      state.lastErrorMessage = null
      return true
    } catch {
      clearSession()
      return false
    } finally {
      state.isRefreshing = false
      refreshPromise = null
    }
  })()

  return refreshPromise
}

async function fetchCurrentCustomer(force = false) {
  if (!state.session) {
    state.currentCustomer = null
    return null
  }

  if (currentCustomerPromise && !force) {
    return currentCustomerPromise
  }

  currentCustomerPromise = (async () => {
    state.isLoadingCustomer = true

    try {
      const customer = await currentCustomerApi.getCurrentCustomer()
      state.currentCustomer = customer
      state.lastErrorMessage = null
      return customer
    } catch (error) {
      if (!state.session || (isApiError(error) && error.status === 401)) {
        return null
      }

      state.lastErrorMessage = error instanceof Error ? error.message : '加载个人信息失败'
      throw error
    } finally {
      state.isLoadingCustomer = false
      currentCustomerPromise = null
    }
  })()

  return currentCustomerPromise
}

async function updateCurrentCustomerProfile(payload: UpdateCurrentCustomerProfilePayload) {
  state.lastErrorMessage = null

  const customer = await currentCustomerApi.updateCurrentCustomerProfile(payload)
  state.currentCustomer = customer

  return customer
}

async function updateCurrentCustomerAvatar(avatarUrl: string | null) {
  state.lastErrorMessage = null

  const payload: UpdateCurrentCustomerAvatarPayload = {
    avatarUrl,
  }

  const customer = await currentCustomerApi.updateCurrentCustomerAvatar(payload)
  state.currentCustomer = customer

  return customer
}

async function logout() {
  const refreshToken = state.session?.refreshToken

  try {
    if (refreshToken) {
      await customerAuthApi.logout({ refreshToken })
    }
  } catch {
    console.warn('Logout failed, clearing session anyway')
  } finally {
    clearSession()
  }
}

function clearSession() {
  state.session = null
  state.currentCustomer = null
  state.lastErrorMessage = null
  clearPersistedCustomerSession()
}

function applySession(session: CustomerSession) {
  state.session = session
  persistCustomerSession(session)
}

function isRefreshTokenExpired(session: CustomerSession) {
  const expiresAt = Date.parse(session.refreshTokenExpiresAt)

  return Number.isNaN(expiresAt) || expiresAt <= Date.now()
}

function mapCustomerSession(response: CustomerLoginResponse): CustomerSession {
  return {
    accessToken: response.accessToken,
    refreshToken: response.refreshToken,
    tokenType: response.tokenType,
    expiresAt: response.expiresAt,
    refreshTokenExpiresAt: response.refreshTokenExpiresAt,
  }
}
