import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

import { useAuthStore } from '@/stores/auth'

const authApi = vi.hoisted(() => ({
  fetchCurrentAuthorization: vi.fn(),
  login: vi.fn(),
  logout: vi.fn(),
  phoneLogin: vi.fn(),
  refresh: vi.fn(),
}))

vi.mock('@/features/auth/api/authApi', () => authApi)

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

function createDeferred<T>() {
  let resolve!: (value: T | PromiseLike<T>) => void
  let reject!: (reason?: unknown) => void

  const promise = new Promise<T>((innerResolve, innerReject) => {
    resolve = innerResolve
    reject = innerReject
  })

  return {
    promise,
    reject,
    resolve,
  }
}

beforeEach(() => {
  setActivePinia(createPinia())

  Object.defineProperty(globalThis, 'localStorage', {
    value: new LocalStorageMock(),
    configurable: true,
    writable: true,
  })

  authApi.login.mockReset()
  authApi.logout.mockReset()
  authApi.phoneLogin.mockReset()
  authApi.refresh.mockReset()
  authApi.fetchCurrentAuthorization.mockReset()
})

describe('auth store', () => {
  it('stores token response and authorization context after login', async () => {
    authApi.login.mockResolvedValue({
      accessToken: 'access-token',
      refreshToken: 'refresh-token',
      tokenType: 'Bearer',
      expiresAt: '2026-04-20T00:00:00Z',
      refreshTokenExpiresAt: '2026-04-27T00:00:00Z',
    })

    authApi.fetchCurrentAuthorization.mockResolvedValue({
      userId: 'user-1',
      userName: 'seed-admin',
      displayName: 'Platform Super Admin',
      roleCodes: ['super_admin'],
      permissionCodes: ['dashboard.view', 'admin_users.create'],
      pagePermissionCodes: ['dashboard.view'],
      buttonPermissionCodes: ['admin_users.create'],
    })

    const authStore = useAuthStore()
    await authStore.login({
      account: 'seed-admin',
      password: 'ExamplePass@123',
    })

    expect(authStore.accessToken).toBe('access-token')
    expect(authStore.displayName).toBe('Platform Super Admin')
    expect(authStore.pagePermissionCodes).toEqual(['dashboard.view'])
    expect(authStore.buttonPermissionCodes).toEqual(['admin_users.create'])
  })

  it('updates token state after refresh', async () => {
    authApi.refresh.mockResolvedValue({
      accessToken: 'new-access-token',
      refreshToken: 'new-refresh-token',
      tokenType: 'Bearer',
      expiresAt: '2026-04-28T00:00:00Z',
      refreshTokenExpiresAt: '2026-05-05T00:00:00Z',
    })

    const authStore = useAuthStore()
    authStore.accessToken = 'old-access-token'
    authStore.refreshToken = 'old-refresh-token'
    authStore.tokenType = 'Bearer'
    authStore.expiresAt = '2026-04-20T00:00:00Z'
    authStore.refreshTokenExpiresAt = '2026-04-27T00:00:00Z'

    await authStore.refresh()

    expect(authApi.refresh).toHaveBeenCalledWith({
      refreshToken: 'old-refresh-token',
    })
    expect(authStore.accessToken).toBe('new-access-token')
    expect(authStore.refreshToken).toBe('new-refresh-token')
    expect(authStore.expiresAt).toBe('2026-04-28T00:00:00Z')
    expect(authStore.refreshTokenExpiresAt).toBe('2026-05-05T00:00:00Z')
  })

  it('stores token response and authorization context after phone login', async () => {
    authApi.phoneLogin.mockResolvedValue({
      accessToken: 'phone-access-token',
      refreshToken: 'phone-refresh-token',
      tokenType: 'Bearer',
      expiresAt: '2026-04-22T00:00:00Z',
      refreshTokenExpiresAt: '2026-04-29T00:00:00Z',
    })

    authApi.fetchCurrentAuthorization.mockResolvedValue({
      userId: 'user-2',
      userName: 'phone_13800138000',
      displayName: 'Phone Login Admin',
      roleCodes: ['ops_admin'],
      permissionCodes: ['dashboard.view', 'message_templates.page'],
      pagePermissionCodes: ['dashboard.view', 'message_templates.page'],
      buttonPermissionCodes: ['message_templates.create'],
    })

    const authStore = useAuthStore()
    await authStore.phoneLogin({
      phoneNumber: '13800138000',
      verificationCode: '666666',
    })

    expect(authApi.phoneLogin).toHaveBeenCalledWith({
      phoneNumber: '13800138000',
      verificationCode: '666666',
    })
    expect(authStore.accessToken).toBe('phone-access-token')
    expect(authStore.displayName).toBe('Phone Login Admin')
    expect(authStore.pagePermissionCodes).toEqual(['dashboard.view', 'message_templates.page'])
    expect(authStore.buttonPermissionCodes).toEqual(['message_templates.create'])
  })

  it('updates persisted current admin identity fields', () => {
    const authStore = useAuthStore()
    authStore.userName = 'legacy-admin'
    authStore.displayName = 'Legacy Admin'

    authStore.updateCurrentAdminIdentity({
      userName: 'profile-admin',
      displayName: 'Profile Admin',
    })

    expect(authStore.userName).toBe('profile-admin')
    expect(authStore.displayName).toBe('Profile Admin')
    expect(JSON.parse(localStorage.getItem('explore.admin.auth') ?? '{}')).toEqual(
      expect.objectContaining({
        userName: 'profile-admin',
        displayName: 'Profile Admin',
      }),
    )
  })

  it('hydrates persisted session before refreshing current authorization', async () => {
    localStorage.setItem(
      'explore.admin.auth',
      JSON.stringify({
        accessToken: 'persisted-access-token',
        refreshToken: 'persisted-refresh-token',
        tokenType: 'Bearer',
        expiresAt: '2026-04-20T00:00:00Z',
        refreshTokenExpiresAt: '2026-04-27T00:00:00Z',
        userId: 'persisted-user',
        userName: 'persisted-admin',
        displayName: 'Persisted Admin',
        roleCodes: ['persisted_role'],
        permissionCodes: ['dashboard.view'],
        pagePermissionCodes: ['dashboard.view'],
        buttonPermissionCodes: ['admin_users.create'],
      }),
    )

    const deferredAuthorization = createDeferred<{
      userId: string
      userName: string
      displayName: string
      roleCodes: string[]
      permissionCodes: string[]
      pagePermissionCodes: string[]
      buttonPermissionCodes: string[]
    }>()

    authApi.fetchCurrentAuthorization.mockReturnValue(deferredAuthorization.promise)

    const authStore = useAuthStore()
    const bootstrapPromise = authStore.bootstrapSession()

    expect(authStore.accessToken).toBe('persisted-access-token')
    expect(authStore.displayName).toBe('Persisted Admin')
    expect(authStore.isAuthenticated).toBe(true)
    expect(authStore.hasHydratedSession).toBe(true)
    expect(authStore.isBootstrappingSession).toBe(true)

    deferredAuthorization.resolve({
      userId: 'user-1',
      userName: 'seed-admin',
      displayName: 'Platform Super Admin',
      roleCodes: ['super_admin'],
      permissionCodes: ['dashboard.view', 'admin_users.create'],
      pagePermissionCodes: ['dashboard.view'],
      buttonPermissionCodes: ['admin_users.create'],
    })

    await bootstrapPromise

    expect(authStore.displayName).toBe('Platform Super Admin')
    expect(authStore.isBootstrappingSession).toBe(false)
  })

  it('clears session when bootstrapping current authorization fails', async () => {
    localStorage.setItem(
      'explore.admin.auth',
      JSON.stringify({
        accessToken: 'persisted-access-token',
        refreshToken: 'persisted-refresh-token',
        tokenType: 'Bearer',
        expiresAt: '2026-04-20T00:00:00Z',
        refreshTokenExpiresAt: '2026-04-27T00:00:00Z',
      }),
    )

    authApi.fetchCurrentAuthorization.mockRejectedValue(new Error('unauthorized'))

    const authStore = useAuthStore()
    await authStore.bootstrapSession()

    expect(authStore.isAuthenticated).toBe(false)
    expect(authStore.accessToken).toBeNull()
    expect(localStorage.getItem('explore.admin.auth')).toBeNull()
  })
})
