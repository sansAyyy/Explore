import type {
  AxiosError,
  AxiosResponse,
  InternalAxiosRequestConfig,
} from 'axios'

import { AxiosHeaders } from 'axios'
import axios from 'axios'
import { ElMessage } from 'element-plus'
import type { Pinia } from 'pinia'
import type { Router } from 'vue-router'

import { extractErrorMessage } from '@/shared/api/error'
import { getPersistedAuthState } from '@/shared/auth/storage'
import { getGatewayBaseUrl } from '@/shared/config/env'
import { useAuthStore } from '@/stores/auth'

declare module 'axios' {
  interface AxiosRequestConfig {
    _retry?: boolean
    _skipAuthRefresh?: boolean
  }

  interface InternalAxiosRequestConfig {
    _retry?: boolean
    _skipAuthRefresh?: boolean
  }
}

export const http = axios.create({
  baseURL: getGatewayBaseUrl(),
  timeout: 10000,
})

let interceptorsInstalled = false
let responseInterceptorId: number | null = null
let refreshPromise: Promise<void> | null = null
let sessionExpiredPromise: Promise<void> | null = null

http.interceptors.request.use((config) => {
  const authState = getPersistedAuthState()

  if (authState?.accessToken) {
    const headers = AxiosHeaders.from(config.headers)
    headers.set('Authorization', `Bearer ${authState.accessToken}`)
    config.headers = headers
  }

  return config
})

function isAuthRequest(config?: InternalAxiosRequestConfig) {
  if (!config) {
    return false
  }

  if (config._skipAuthRefresh) {
    return true
  }

  const url = config.url ?? ''
  return /^\/(?:[^/]+\/)?api\/admin-auth\//.test(url)
}

function hasRefreshToken() {
  return Boolean(getPersistedAuthState()?.refreshToken)
}

function getRefreshPromise(pinia: Pinia) {
  if (!refreshPromise) {
    const authStore = useAuthStore(pinia)

    refreshPromise = authStore
      .refresh()
      .then(() => undefined)
      .finally(() => {
        refreshPromise = null
      })
  }

  return refreshPromise
}

function handleSessionExpired({
  authStore,
  router,
}: {
  authStore: ReturnType<typeof useAuthStore>
  router: Router
}) {
  if (!sessionExpiredPromise) {
    sessionExpiredPromise = (async () => {
      authStore.clearSession()
      ElMessage.error('登录已过期，请重新登录')

      if (router.currentRoute.value.name !== 'login') {
        const redirectPath = router.currentRoute.value.fullPath
        await router.replace({
          name: 'login',
          query: redirectPath ? { redirect: redirectPath } : undefined,
        })
      }
    })().finally(() => {
      sessionExpiredPromise = null
    })
  }

  return sessionExpiredPromise
}

function shouldAttemptRefresh({
  config,
  status,
}: {
  config?: InternalAxiosRequestConfig
  status?: number
}) {
  if (status !== 401 || !config) {
    return false
  }

  if (isAuthRequest(config)) {
    return false
  }

  if (config._retry) {
    return false
  }

  if (!hasRefreshToken()) {
    return false
  }

  return true
}

export function installHttpInterceptors({ pinia, router }: { pinia: Pinia; router: Router }) {
  if (interceptorsInstalled) {
    return
  }

  interceptorsInstalled = true

  responseInterceptorId = http.interceptors.response.use(
    (response: AxiosResponse) => response,
    async (error: AxiosError) => {
      const status = error.response?.status
      const authStore = useAuthStore(pinia)
      const originalRequest = error.config as InternalAxiosRequestConfig | undefined

      if (shouldAttemptRefresh({ config: originalRequest, status })) {
        try {
          await getRefreshPromise(pinia)

          if (!originalRequest) {
            return Promise.reject(error)
          }

          originalRequest._retry = true
          return http(originalRequest)
        } catch {
          await handleSessionExpired({ authStore, router })
          return Promise.reject(error)
        }
      }

      if (status === 401 && originalRequest && !isAuthRequest(originalRequest)) {
        await handleSessionExpired({ authStore, router })
      }

      if (status === 403) {
        ElMessage.error(extractErrorMessage(error, '暂无权限访问该资源'))
      }

      return Promise.reject(error)
    },
  )
}

export function resetHttpInterceptorsForTest() {
  if (responseInterceptorId !== null) {
    http.interceptors.response.eject(responseInterceptorId)
    responseInterceptorId = null
  }

  interceptorsInstalled = false
  refreshPromise = null
  sessionExpiredPromise = null
}
