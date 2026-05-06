<template>
  <view class="page-shell login-page">
    <view class="surface-card form-card">
      <text class="title">登录</text>

      <input
        v-model.trim="form.phoneNumber"
        class="form-input"
        type="number"
        maxlength="11"
        placeholder="手机号"
      />

      <view class="code-row">
        <input
          v-model.trim="form.verificationCode"
          class="form-input code-input"
          type="number"
          maxlength="6"
          placeholder="验证码"
        />
        <button class="secondary-action code-button" :disabled="isSendingCode" @click="sendCode">
          {{ isSendingCode ? '发送中' : '获取验证码' }}
        </button>
      </view>

      <button class="primary-action" :disabled="isSubmitting" @click="submit">
        {{ isSubmitting ? '登录中' : '登录' }}
      </button>
    </view>
  </view>
</template>

<script setup lang="ts">
import { onShow } from '@dcloudio/uni-app'
import { reactive, ref } from 'vue'

import { useCustomerSession } from '@/features/customer-account/composables/useCustomerSession'
import {
  createEmptyPhoneLoginForm,
  validatePhoneNumber,
  validateVerificationCode,
} from '@/features/customer-account/models/phoneLoginForm'

const { isAuthenticated, sendPhoneLoginCode, loginWithPhone } = useCustomerSession()

const form = reactive(createEmptyPhoneLoginForm())
const isSendingCode = ref(false)
const isSubmitting = ref(false)

onShow(() => {
  if (isAuthenticated.value) {
    uni.switchTab({ url: '/features/customer-account/pages/profile/index' })
  }
})

async function sendCode() {
  const validationMessage = validatePhoneNumber(form.phoneNumber)
  if (validationMessage) return showToast(validationMessage)

  isSendingCode.value = true
  try {
    await sendPhoneLoginCode(form.phoneNumber)
    uni.showToast({ title: '验证码已发送', icon: 'success' })
  } catch (error) {
    showToast(error)
  } finally {
    isSendingCode.value = false
  }
}

async function submit() {
  const phoneValidationMessage = validatePhoneNumber(form.phoneNumber)
  if (phoneValidationMessage) return showToast(phoneValidationMessage)

  const verificationCodeValidationMessage = validateVerificationCode(form.verificationCode)
  if (verificationCodeValidationMessage) return showToast(verificationCodeValidationMessage)

  isSubmitting.value = true
  try {
    await loginWithPhone({
      phoneNumber: form.phoneNumber,
      verificationCode: form.verificationCode,
    })
    uni.showToast({ title: '登录成功', icon: 'success' })
    uni.switchTab({ url: '/features/customer-account/pages/profile/index' })
  } catch (error) {
    showToast(error)
  } finally {
    isSubmitting.value = false
  }
}

function showToast(error: unknown) {
  const message = error instanceof Error ? error.message : String(error)
  uni.showToast({ title: message || '操作失败，请稍后重试', icon: 'none' })
}
</script>

<style scoped>
.login-page {
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

.code-row {
  display: flex;
  align-items: center;
  gap: 12rpx;
}

.code-input {
  flex: 1;
}

.code-button {
  flex-shrink: 0;
  width: 190rpx;
  padding: 0;
}
</style>
