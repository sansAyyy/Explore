import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

import { useCurrentAdminProfile } from '@/features/current-admin/composables/useCurrentAdminProfile'
import { useAuthStore } from '@/stores/auth'

const currentAdminApi = vi.hoisted(() => ({
  changeCurrentAdminPassword: vi.fn(),
  getCurrentAdmin: vi.fn(),
  updateCurrentAdminProfile: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
  success: vi.fn(),
}))

vi.mock('@/features/current-admin/api/currentAdminApi', () => currentAdminApi)
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

describe('useCurrentAdminProfile', () => {
  beforeEach(() => {
    setActivePinia(createPinia())

    Object.defineProperty(globalThis, 'localStorage', {
      value: new LocalStorageMock(),
      configurable: true,
      writable: true,
    })

    currentAdminApi.changeCurrentAdminPassword.mockReset()
    currentAdminApi.getCurrentAdmin.mockReset()
    currentAdminApi.updateCurrentAdminProfile.mockReset()
    message.error.mockReset()
    message.success.mockReset()
  })

  it('loads the current admin profile', async () => {
    currentAdminApi.getCurrentAdmin.mockResolvedValue({
      id: 'admin-1',
      userName: 'ops_admin',
      email: 'ops@explore.local',
      phoneNumber: '13800138000',
      displayName: 'Operations Admin',
      isActive: true,
      createdAt: '2026-04-01T00:00:00Z',
      updatedAt: '2026-04-20T00:00:00Z',
      lastLoginAt: '2026-04-28T09:00:00Z',
      version: 3,
    })

    const profile = useCurrentAdminProfile()
    const promise = profile.loadCurrentAdmin()

    expect(profile.isLoading.value).toBe(true)
    await promise

    expect(currentAdminApi.getCurrentAdmin).toHaveBeenCalledTimes(1)
    expect(profile.currentAdmin.value?.displayName).toBe('Operations Admin')
    expect(profile.hasLoaded.value).toBe(true)
    expect(profile.isLoading.value).toBe(false)
    expect(profile.lastLoadedAt.value).not.toBeNull()
    expect(profile.loadErrorMessage.value).toBe('')
  })

  it('updates profile data and syncs the auth store identity', async () => {
    currentAdminApi.updateCurrentAdminProfile.mockResolvedValue({
      id: 'admin-1',
      userName: 'profile-admin',
      email: 'profile@explore.local',
      phoneNumber: '13800138000',
      displayName: 'Profile Admin',
      isActive: true,
      createdAt: '2026-04-01T00:00:00Z',
      updatedAt: '2026-04-28T10:00:00Z',
      lastLoginAt: '2026-04-28T09:00:00Z',
      version: 4,
    })

    const authStore = useAuthStore()
    const profile = useCurrentAdminProfile()
    await profile.saveProfile({
      userName: 'profile-admin',
      email: 'profile@explore.local',
      displayName: 'Profile Admin',
    })

    expect(currentAdminApi.updateCurrentAdminProfile).toHaveBeenCalledWith({
      userName: 'profile-admin',
      email: 'profile@explore.local',
      displayName: 'Profile Admin',
    })
    expect(profile.currentAdmin.value?.userName).toBe('profile-admin')
    expect(authStore.userName).toBe('profile-admin')
    expect(authStore.displayName).toBe('Profile Admin')
    expect(message.success).toHaveBeenCalledWith('个人资料已更新')
  })

  it('changes the password and clears pending state on success', async () => {
    currentAdminApi.changeCurrentAdminPassword.mockResolvedValue(undefined)
    const profile = useCurrentAdminProfile()

    const promise = profile.updatePassword({
      currentPassword: 'ExamplePass@123',
      newPassword: 'Profile@654321',
    })

    expect(profile.isPasswordSubmitting.value).toBe(true)
    await promise

    expect(currentAdminApi.changeCurrentAdminPassword).toHaveBeenCalledWith({
      currentPassword: 'ExamplePass@123',
      newPassword: 'Profile@654321',
    })
    expect(message.success).toHaveBeenCalledWith('登录密码已更新')
    expect(profile.isPasswordSubmitting.value).toBe(false)
  })

  it('shows an error when updating profile fails', async () => {
    currentAdminApi.updateCurrentAdminProfile.mockRejectedValue(new Error('profile failed'))
    const profile = useCurrentAdminProfile()

    await expect(
      profile.saveProfile({
        userName: 'broken',
        email: 'broken@explore.local',
        displayName: 'Broken',
      }),
    ).rejects.toThrow('profile failed')

    expect(message.error).toHaveBeenCalledWith('profile failed')
    expect(profile.isProfileSubmitting.value).toBe(false)
  })

  it('stores load error state when the initial profile request fails', async () => {
    currentAdminApi.getCurrentAdmin.mockRejectedValue(new Error('load failed'))
    const profile = useCurrentAdminProfile()

    await expect(profile.loadCurrentAdmin()).rejects.toThrow('load failed')

    expect(profile.hasLoaded.value).toBe(true)
    expect(profile.currentAdmin.value).toBeNull()
    expect(profile.lastLoadedAt.value).toBeNull()
    expect(profile.loadErrorMessage.value).toBe('load failed')
    expect(message.error).toHaveBeenCalledWith('load failed')
  })
})
