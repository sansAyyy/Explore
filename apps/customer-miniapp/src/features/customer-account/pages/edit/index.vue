<template>
  <view class="page-shell edit-page">
    <view class="surface-card form-card">
      <text class="title">编辑资料</text>

      <view v-if="state.isRestoring || state.isLoadingCustomer" class="muted-panel status-box">
        <text class="status-copy">加载中</text>
      </view>

      <template v-else>
        <view class="avatar-section">
          <view class="avatar-shell" @click="selectAvatar">
            <image v-if="avatarPreviewUrl" class="avatar-image" :src="avatarPreviewUrl" mode="aspectFill" />
            <view v-else class="avatar-fallback">
              <text class="avatar-text">{{ avatarText }}</text>
            </view>
          </view>

          <button class="avatar-trigger" :disabled="isUploadingAvatar" @click="selectAvatar">
            {{ isUploadingAvatar ? '上传中' : '更换头像' }}
          </button>
        </view>

        <view v-if="state.currentCustomer" class="muted-panel readonly-panel">
          <text class="readonly-value">{{ state.currentCustomer.phoneNumber }}</text>
        </view>

        <input v-model.trim="form.nickName" class="form-input" maxlength="64" placeholder="昵称" />
        <input v-model.trim="form.email" class="form-input" maxlength="256" placeholder="邮箱" />

        <button class="primary-action" :disabled="isSubmitting" @click="submit">
          {{ isSubmitting ? '保存中' : '保存' }}
        </button>
      </template>
    </view>
  </view>
</template>

<script setup lang="ts">
import { onLoad } from '@dcloudio/uni-app'
import { computed, reactive, ref } from 'vue'

import { useCustomerSession } from '@/features/customer-account/composables/useCustomerSession'
import {
  createEmptyProfileForm,
  createProfileFormFromCustomer,
  toUpdateCurrentCustomerProfilePayload,
  validateProfileForm,
} from '@/features/customer-account/models/profileForm'
import { uploadAvatar } from '@/features/customer-account/services/avatarUpload'
import { resolveAvatarUrl } from '@/features/customer-account/utils/avatar'

const { state, restoreSession, fetchCurrentCustomer, updateCurrentCustomerProfile, updateCurrentCustomerAvatar } =
  useCustomerSession()

const form = reactive(createEmptyProfileForm())
const isSubmitting = ref(false)
const isUploadingAvatar = ref(false)
const localAvatarPreviewUrl = ref('')

const avatarPreviewUrl = computed(() => {
  if (localAvatarPreviewUrl.value) {
    return localAvatarPreviewUrl.value
  }

  return resolveAvatarUrl(state.currentCustomer?.avatarUrl)
})

const avatarText = computed(() => {
  const name = form.nickName.trim() || state.currentCustomer?.nickName?.trim()

  if (name) {
    return name.slice(0, 1).toUpperCase()
  }

  return '头'
})

onLoad(() => {
  void initialize()
})

async function initialize() {
  try {
    await restoreSession()

    if (!state.session) {
      redirectToProfile('请先登录后再编辑资料')
      return
    }

    if (!state.currentCustomer) {
      await fetchCurrentCustomer(true)
    }

    if (!state.currentCustomer) {
      redirectToProfile('当前资料暂不可用')
      return
    }

    Object.assign(form, createProfileFormFromCustomer(state.currentCustomer))
  } catch (error) {
    showToast(error)
  }
}

async function selectAvatar() {
  if (isUploadingAvatar.value) {
    return
  }

  try {
    const filePath = await chooseImage()
    if (!filePath) {
      return
    }

    localAvatarPreviewUrl.value = filePath
    isUploadingAvatar.value = true

    const avatarUrl = await uploadAvatar(filePath)
    await updateCurrentCustomerAvatar(avatarUrl)
    localAvatarPreviewUrl.value = ''

    uni.showToast({
      title: '头像已更新',
      icon: 'success',
    })
  } catch (error) {
    if (isUserCancelled(error)) {
      return
    }

    localAvatarPreviewUrl.value = ''
    showToast(error)
  } finally {
    isUploadingAvatar.value = false
  }
}

async function submit() {
  const validationMessage = validateProfileForm(form)
  if (validationMessage) return showToast(validationMessage)

  isSubmitting.value = true
  try {
    await updateCurrentCustomerProfile(toUpdateCurrentCustomerProfilePayload(form))
    uni.showToast({ title: '资料已更新', icon: 'success' })
    uni.switchTab({ url: '/features/customer-account/pages/profile/index' })
  } catch (error) {
    showToast(error)
  } finally {
    isSubmitting.value = false
  }
}

function chooseImage() {
  return new Promise<string>((resolve, reject) => {
    uni.chooseImage({
      count: 1,
      sizeType: ['compressed'],
      sourceType: ['album', 'camera'],
      success: (result) => {
        resolve(result.tempFilePaths?.[0] ?? '')
      },
      fail: (error) => {
        reject(error)
      },
    })
  })
}

function isUserCancelled(error: unknown) {
  const message = getErrorMessage(error)
  return /cancel/i.test(message)
}

function redirectToProfile(message: string) {
  uni.showToast({ title: message, icon: 'none' })
  setTimeout(() => {
    uni.switchTab({ url: '/features/customer-account/pages/profile/index' })
  }, 300)
}

function showToast(error: unknown) {
  const message = getErrorMessage(error)
  uni.showToast({ title: message || '操作失败，请稍后重试', icon: 'none' })
}

function getErrorMessage(error: unknown) {
  if (error instanceof Error && error.message) {
    return error.message
  }

  if (typeof error === 'string') {
    return error
  }

  if (error && typeof error === 'object') {
    const candidate = error as {
      errMsg?: unknown
      message?: unknown
    }

    if (typeof candidate.errMsg === 'string' && candidate.errMsg) {
      return candidate.errMsg
    }

    if (typeof candidate.message === 'string' && candidate.message) {
      return candidate.message
    }
  }

  return ''
}
</script>

<style scoped>
.edit-page {
  display: flex;
  flex-direction: column;
}

.form-card {
  display: flex;
  flex-direction: column;
  gap: 18rpx;
  padding: 28rpx 24rpx;
}

.title {
  font-size: 30rpx;
  font-weight: 600;
  color: var(--text-primary);
}

.avatar-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 14rpx;
  padding-bottom: 8rpx;
}

.avatar-shell {
  width: 112rpx;
  height: 112rpx;
  padding: 5rpx;
  border-radius: 999rpx;
  background: linear-gradient(135deg, rgba(47, 107, 98, 0.2) 0%, rgba(32, 58, 55, 0.08) 100%);
}

.avatar-image,
.avatar-fallback {
  width: 100%;
  height: 100%;
  border-radius: 999rpx;
}

.avatar-fallback {
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.92);
}

.avatar-text {
  font-size: 34rpx;
  font-weight: 700;
  color: var(--brand-accent);
}

.avatar-trigger {
  min-width: 160rpx;
  height: 56rpx;
  padding: 0 20rpx;
  border: none;
  background: transparent;
  color: var(--brand-accent);
  font-size: 24rpx;
  font-weight: 600;
}

.status-box,
.readonly-panel {
  padding: 18rpx 20rpx;
}

.status-copy,
.readonly-value {
  font-size: 24rpx;
  color: var(--text-muted);
}
</style>
