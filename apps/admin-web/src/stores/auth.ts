import { defineStore } from 'pinia'
import { computed, reactive, ref, toRefs } from 'vue'

import * as authApi from '@/features/auth/api/authApi'
import type {
  AdminAuthState,
  AdminLoginRequest,
  AdminPhoneLoginRequest,
  CurrentAdminAuthorizationResponse,
} from '@/features/auth/types/auth'
import {
  clearPersistedAuthState,
  createEmptyAuthState,
  getPersistedAuthState,
  persistAuthState,
} from '@/shared/auth/storage'
import { hasButtonPermission, hasPagePermission } from '@/shared/auth/permissions'

function cloneAuthState(state: AdminAuthState): AdminAuthState {
  return {
    ...state,
    roleCodes: [...state.roleCodes],
    permissionCodes: [...state.permissionCodes],
    pagePermissionCodes: [...state.pagePermissionCodes],
    buttonPermissionCodes: [...state.buttonPermissionCodes],
  }
}

export const useAuthStore = defineStore('auth', () => {
  const state = reactive<AdminAuthState>(createEmptyAuthState())
  const hasHydratedSession = ref(false)
  const hasBootstrappedSession = ref(false)
  const isRefreshingAuthorization = ref(false)
  let bootstrapPromise: Promise<void> | null = null

  const isAuthenticated = computed(() => Boolean(state.accessToken))
  const isBootstrappingSession = computed(
    () => hasHydratedSession.value && isAuthenticated.value && isRefreshingAuthorization.value,
  )

  function persistCurrentState() {
    persistAuthState(cloneAuthState(state))
  }

  function applyTokenState(payload: {
    accessToken: string
    refreshToken: string
    tokenType: string
    expiresAt: string
    refreshTokenExpiresAt: string
  }) {
    state.accessToken = payload.accessToken
    state.refreshToken = payload.refreshToken
    state.tokenType = payload.tokenType
    state.expiresAt = payload.expiresAt
    state.refreshTokenExpiresAt = payload.refreshTokenExpiresAt
    persistCurrentState()
  }

  function applyAuthorizationState(payload: CurrentAdminAuthorizationResponse) {
    state.userId = payload.userId
    state.userName = payload.userName
    state.displayName = payload.displayName
    state.roleCodes = [...payload.roleCodes]
    state.permissionCodes = [...payload.permissionCodes]
    state.pagePermissionCodes = [...payload.pagePermissionCodes]
    state.buttonPermissionCodes = [...payload.buttonPermissionCodes]
    persistCurrentState()
  }

  async function fetchCurrentAuthorization() {
    const authorization = await authApi.fetchCurrentAuthorization()
    applyAuthorizationState(authorization)
    return authorization
  }

  function hydrateSession() {
    if (hasHydratedSession.value) {
      return
    }

    const persistedState = getPersistedAuthState()

    if (persistedState?.accessToken) {
      Object.assign(state, cloneAuthState(persistedState))
    }

    hasHydratedSession.value = true
  }

  async function completeLogin(tokenResult: {
    accessToken: string
    refreshToken: string
    tokenType: string
    expiresAt: string
    refreshTokenExpiresAt: string
  }) {
    applyTokenState(tokenResult)
    await fetchCurrentAuthorization()
  }

  async function login(payload: AdminLoginRequest) {
    const tokenResult = await authApi.login(payload)
    await completeLogin(tokenResult)
  }

  async function phoneLogin(payload: AdminPhoneLoginRequest) {
    const tokenResult = await authApi.phoneLogin(payload)
    await completeLogin(tokenResult)
  }

  async function refresh() {
    if (!state.refreshToken) {
      throw new Error('Missing refresh token.')
    }

    const tokenResult = await authApi.refresh({
      refreshToken: state.refreshToken,
    })

    applyTokenState(tokenResult)
    return tokenResult
  }

  async function logout() {
    try {
      if (state.refreshToken) {
        await authApi.logout({
          refreshToken: state.refreshToken,
        })
      }
    } catch (error) {
      console.error('Error occurred while logging out:', error)
    } finally {
      clearSession()
    }
  }

  async function bootstrapSession() {
    hydrateSession()

    if (hasBootstrappedSession.value) {
      return bootstrapPromise ?? Promise.resolve()
    }

    hasBootstrappedSession.value = true

    if (!state.accessToken) {
      return
    }

    if (bootstrapPromise) {
      return bootstrapPromise
    }

    isRefreshingAuthorization.value = true
    bootstrapPromise = fetchCurrentAuthorization()
      .then(() => undefined)
      .catch(() => {
        clearSession()
      })
      .finally(() => {
        isRefreshingAuthorization.value = false
        bootstrapPromise = null
      })

    return bootstrapPromise
  }

  function clearSession() {
    Object.assign(state, createEmptyAuthState())
    clearPersistedAuthState()
  }

  function canAccessPage(permissionCode?: string) {
    return hasPagePermission(state.pagePermissionCodes, permissionCode)
  }

  function canUseButton(permissionCode: string) {
    return hasButtonPermission(state.buttonPermissionCodes, permissionCode)
  }

  function updateCurrentAdminIdentity(payload: {
    userName: string
    displayName: string
  }) {
    state.userName = payload.userName
    state.displayName = payload.displayName
    persistCurrentState()
  }

  return {
    ...toRefs(state),
    bootstrapSession,
    clearSession,
    fetchCurrentAuthorization,
    hasHydratedSession,
    hasButtonPermission: canUseButton,
    hasPagePermission: canAccessPage,
    hydrateSession,
    isAuthenticated,
    isBootstrappingSession,
    isRefreshingAuthorization,
    login,
    logout,
    phoneLogin,
    refresh,
    updateCurrentAdminIdentity,
  }
})
