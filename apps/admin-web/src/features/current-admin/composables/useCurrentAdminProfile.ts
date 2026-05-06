import { ElMessage } from 'element-plus'
import { ref } from 'vue'

import {
  changeCurrentAdminPassword,
  getCurrentAdmin,
  updateCurrentAdminProfile,
} from '@/features/current-admin/api/currentAdminApi'
import type {
  ChangeCurrentAdminPasswordRequest,
  CurrentAdminResponse,
  UpdateCurrentAdminProfileRequest,
} from '@/features/current-admin/types/currentAdmin'
import { extractErrorMessage } from '@/shared/api/error'
import { useAuthStore } from '@/stores/auth'

export function useCurrentAdminProfile() {
  const authStore = useAuthStore()
  const currentAdmin = ref<CurrentAdminResponse | null>(null)
  const isLoading = ref(false)
  const isProfileSubmitting = ref(false)
  const isPasswordSubmitting = ref(false)
  const hasLoaded = ref(false)
  const lastLoadedAt = ref<string | null>(null)
  const loadErrorMessage = ref('')

  async function loadCurrentAdmin() {
    loadErrorMessage.value = ''
    isLoading.value = true

    try {
      const profile = await getCurrentAdmin()
      currentAdmin.value = profile
      hasLoaded.value = true
      lastLoadedAt.value = new Date().toISOString()
      return profile
    } catch (error) {
      hasLoaded.value = true
      loadErrorMessage.value = extractErrorMessage(error, '加载个人中心失败')
      ElMessage.error(loadErrorMessage.value)
      throw error
    } finally {
      isLoading.value = false
    }
  }

  async function saveProfile(payload: UpdateCurrentAdminProfileRequest) {
    isProfileSubmitting.value = true

    try {
      const profile = await updateCurrentAdminProfile(payload)
      currentAdmin.value = profile
      authStore.updateCurrentAdminIdentity({
        userName: profile.userName,
        displayName: profile.displayName,
      })
      ElMessage.success('个人资料已更新')
      return profile
    } catch (error) {
      ElMessage.error(extractErrorMessage(error, '更新个人资料失败'))
      throw error
    } finally {
      isProfileSubmitting.value = false
    }
  }

  async function updatePassword(payload: ChangeCurrentAdminPasswordRequest) {
    isPasswordSubmitting.value = true

    try {
      await changeCurrentAdminPassword(payload)
      ElMessage.success('登录密码已更新')
    } catch (error) {
      ElMessage.error(extractErrorMessage(error, '更新登录密码失败'))
      throw error
    } finally {
      isPasswordSubmitting.value = false
    }
  }

  return {
    currentAdmin,
    hasLoaded,
    isLoading,
    isPasswordSubmitting,
    isProfileSubmitting,
    lastLoadedAt,
    loadCurrentAdmin,
    loadErrorMessage,
    saveProfile,
    updatePassword,
  }
}
