import type { AxiosResponse, InternalAxiosRequestConfig } from 'axios'

import { AxiosError, AxiosHeaders } from 'axios'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

import * as authApi from '@/features/auth/api/authApi'
import type { AdminAuthState } from '@/features/auth/types/auth'
import { buildServiceApiPath, gatewayServices } from '@/shared/api/gateway'
import {
  createEmptyAuthState,
  persistAuthState,
} from '@/shared/auth/storage'
import {
  http,
  installHttpInterceptors,
  resetHttpInterceptorsForTest,
} from '@/shared/api/http'
import { useAuthStore } from '@/stores/auth'

const message = vi.hoisted(() => ({
  error: vi.fn(),
}))

vi.mock('element-plus', () => ({
  ElMessage: message,
}))

class LocalStorageMock {
  private store = new Map<string, string>()

  getItem(key: string) {
    return this.store.get(key) ?? null
  }

  removeItem(key: string) {
    this.store.delete(key)
  }

  setItem(key: string, value: string) {
    this.store.set(key, value)
  }
}

function createRouterMock(path = '/dashboard') {
  return {
    currentRoute: {
      value: {
        name: 'dashboard',
        fullPath: path,
      },
    },
    replace: vi.fn(async (location: unknown) => location),
  }
}

function createResponse<T = unknown>(
  config: InternalAxiosRequestConfig,
  data: T,
  status = 200,
): AxiosResponse<T> {
  return {
    data,
    status,
    statusText: status === 200 ? 'OK' : 'Error',
    headers: {},
    config,
  }
}

function createHttpError(
  config: InternalAxiosRequestConfig,
  status: number,
  title = 'Unauthorized',
) {
  return new AxiosError(
    title,
    undefined,
    config,
    null,
    createResponse(config, { title }, status),
  )
}

function createAuthState(overrides: Partial<AdminAuthState> = {}): AdminAuthState {
  return {
    ...createEmptyAuthState(),
    accessToken: 'old-access-token',
    refreshToken: 'refresh-token',
    tokenType: 'Bearer',
    expiresAt: '2026-04-20T00:00:00Z',
    refreshTokenExpiresAt: '2026-04-27T00:00:00Z',
    ...overrides,
  }
}

function applyAuthState(authStore: ReturnType<typeof useAuthStore>, overrides: Partial<AdminAuthState> = {}) {
  const authState = createAuthState(overrides)
  Object.assign(authStore, authState)
  persistAuthState(authState)
}

function getAuthorizationHeader(config: InternalAxiosRequestConfig) {
  const value = AxiosHeaders.from(config.headers).get('Authorization')
  return typeof value === 'string' ? value : null
}

function createServicePath(path: string) {
  return buildServiceApiPath(gatewayServices.adminIdentity, path)
}

async function waitForAssertion(assertion: () => void, timeoutMs = 200) {
  const startedAt = Date.now()

  while (true) {
    try {
      assertion()
      return
    } catch (error) {
      if (Date.now() - startedAt >= timeoutMs) {
        throw error
      }

      await new Promise((resolve) => setTimeout(resolve, 0))
    }
  }
}

describe('http auth refresh interceptors', () => {
  const originalAdapter = http.defaults.adapter

  beforeEach(() => {
    setActivePinia(createPinia())

    Object.defineProperty(globalThis, 'localStorage', {
      value: new LocalStorageMock(),
      configurable: true,
      writable: true,
    })

    message.error.mockReset()
    resetHttpInterceptorsForTest()
  })

  afterEach(() => {
    http.defaults.adapter = originalAdapter
    resetHttpInterceptorsForTest()
  })

  it('refreshes once and retries a failed request', async () => {
    const pinia = createPinia()
    const authStore = useAuthStore(pinia)
    applyAuthState(authStore)

    const router = createRouterMock()
    const refreshSpy = vi.spyOn(authStore, 'refresh').mockImplementation(async () => {
      applyAuthState(authStore, {
        accessToken: 'new-access-token',
        refreshToken: 'new-refresh-token',
        expiresAt: '2026-04-28T00:00:00Z',
        refreshTokenExpiresAt: '2026-05-05T00:00:00Z',
      })

      return {
        accessToken: 'new-access-token',
        refreshToken: 'new-refresh-token',
        tokenType: 'Bearer',
        expiresAt: '2026-04-28T00:00:00Z',
        refreshTokenExpiresAt: '2026-05-05T00:00:00Z',
      }
    })

    let protectedRequestCount = 0
    const authorizationHeaders: Array<string | null> = []

    http.defaults.adapter = vi.fn(async (config) => {
      if (!config.url) {
        throw new Error('Missing request url.')
      }

      authorizationHeaders.push(getAuthorizationHeader(config))

      if (config.url === '/api/protected') {
        protectedRequestCount += 1

        if (protectedRequestCount === 1) {
          throw createHttpError(config, 401)
        }

        return createResponse(config, { ok: true })
      }

      throw new Error(`Unexpected request url: ${config.url}`)
    })

    installHttpInterceptors({
      pinia,
      router: router as never,
    })

    const result = await http.get<{ ok: boolean }>('/api/protected')

    expect(result.data).toEqual({ ok: true })
    expect(refreshSpy).toHaveBeenCalledTimes(1)
    expect(protectedRequestCount).toBe(2)
    expect(authorizationHeaders).toEqual([
      'Bearer old-access-token',
      'Bearer new-access-token',
    ])
  })

  it('queues concurrent 401 requests behind a single refresh', async () => {
    const pinia = createPinia()
    const authStore = useAuthStore(pinia)
    applyAuthState(authStore)

    const router = createRouterMock()
    let resolveRefresh: (() => void) | null = null
    const refreshGate = new Promise<void>((resolve) => {
      resolveRefresh = resolve
    })

    const refreshSpy = vi.spyOn(authStore, 'refresh').mockImplementation(async () => {
      await refreshGate
      applyAuthState(authStore, {
        accessToken: 'new-access-token',
        refreshToken: 'new-refresh-token',
        expiresAt: '2026-04-28T00:00:00Z',
        refreshTokenExpiresAt: '2026-05-05T00:00:00Z',
      })

      return {
        accessToken: 'new-access-token',
        refreshToken: 'new-refresh-token',
        tokenType: 'Bearer',
        expiresAt: '2026-04-28T00:00:00Z',
        refreshTokenExpiresAt: '2026-05-05T00:00:00Z',
      }
    })

    const requestAttempts = new Map<string, number>()

    http.defaults.adapter = vi.fn(async (config) => {
      if (!config.url) {
        throw new Error('Missing request url.')
      }

      const count = (requestAttempts.get(config.url) ?? 0) + 1
      requestAttempts.set(config.url, count)

      if (config.url === '/api/one' || config.url === '/api/two') {
        if (count === 1) {
          throw createHttpError(config, 401)
        }

        return createResponse(config, { url: config.url })
      }

      throw new Error(`Unexpected request url: ${config.url}`)
    })

    installHttpInterceptors({
      pinia,
      router: router as never,
    })

    const requests = Promise.all([
      http.get('/api/one'),
      http.get('/api/two'),
    ])

    await waitForAssertion(() => {
      expect(refreshSpy).toHaveBeenCalledTimes(1)
    })

    resolveRefresh!()
    const [firstResult, secondResult] = await requests

    expect(firstResult.data).toEqual({ url: '/api/one' })
    expect(secondResult.data).toEqual({ url: '/api/two' })
    expect(refreshSpy).toHaveBeenCalledTimes(1)
    expect(requestAttempts.get('/api/one')).toBe(2)
    expect(requestAttempts.get('/api/two')).toBe(2)
  })

  it('clears session and redirects to login when refresh fails', async () => {
    const pinia = createPinia()
    const authStore = useAuthStore(pinia)
    applyAuthState(authStore)

    const router = createRouterMock('/admin-users?page=2')
    vi.spyOn(authStore, 'refresh').mockRejectedValue(new Error('refresh failed'))

    http.defaults.adapter = vi.fn(async (config) => {
      if (config.url === '/api/protected') {
        throw createHttpError(config, 401)
      }

      throw new Error(`Unexpected request url: ${config.url}`)
    })

    installHttpInterceptors({
      pinia,
      router: router as never,
    })

    await expect(http.get('/api/protected')).rejects.toBeInstanceOf(AxiosError)

    expect(authStore.accessToken).toBeNull()
    expect(authStore.refreshToken).toBeNull()
    expect(router.replace).toHaveBeenCalledWith({
      name: 'login',
      query: { redirect: '/admin-users?page=2' },
    })
    expect(message.error).toHaveBeenCalledWith('登录已过期，请重新登录')
  })

  it('does not attempt refresh for auth requests marked to skip refresh', async () => {
    const pinia = createPinia()
    const authStore = useAuthStore(pinia)
    applyAuthState(authStore)

    const router = createRouterMock()
    const refreshSpy = vi.spyOn(authStore, 'refresh')
    const refreshUrl = createServicePath('/api/admin-auth/refresh')

    http.defaults.adapter = vi.fn(async (config) => {
      if (config.url === refreshUrl) {
        throw createHttpError(config, 401)
      }

      throw new Error(`Unexpected request url: ${config.url}`)
    })

    installHttpInterceptors({
      pinia,
      router: router as never,
    })

    await expect(
      authApi.refresh({
        refreshToken: 'refresh-token',
      }),
    ).rejects.toBeInstanceOf(AxiosError)

    expect(refreshSpy).not.toHaveBeenCalled()
    expect(router.replace).not.toHaveBeenCalled()
  })

  it('does not attempt a second refresh for requests already retried once', async () => {
    const pinia = createPinia()
    const authStore = useAuthStore(pinia)
    applyAuthState(authStore)

    const router = createRouterMock('/dashboard')
    const refreshSpy = vi.spyOn(authStore, 'refresh')

    http.defaults.adapter = vi.fn(async (config) => {
      if (config.url === '/api/protected') {
        throw createHttpError(config, 401)
      }

      throw new Error(`Unexpected request url: ${config.url}`)
    })

    installHttpInterceptors({
      pinia,
      router: router as never,
    })

    await expect(
      http.get('/api/protected', {
        _retry: true,
      }),
    ).rejects.toBeInstanceOf(AxiosError)

    expect(refreshSpy).not.toHaveBeenCalled()
    expect(router.replace).toHaveBeenCalledWith({
      name: 'login',
      query: { redirect: '/dashboard' },
    })
  })
})
