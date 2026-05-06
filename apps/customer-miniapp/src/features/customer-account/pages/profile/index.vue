<template>
  <view class="page-shell profile-page">
    <view class="surface-card header-card" :class="{ 'header-card--guest': showLoginHint }">
      <view v-if="showLoginHint" class="guest-hero">
        <view class="guest-copy">
          <text class="profile-name profile-name--guest">登录后查看账户信息</text>
        </view>
        <view class="avatar-shell avatar-shell--guest">
          <view class="avatar-fallback avatar-fallback--guest">
            <text class="avatar-text">我</text>
          </view>
        </view>
        <button class="primary-action login-action" @click="goToLogin">立即登录</button>
      </view>

      <template v-else>
        <view class="avatar-shell">
          <image v-if="avatarImageUrl" class="avatar-image" :src="avatarImageUrl" mode="aspectFill" />
          <view v-else class="avatar-fallback">
            <text class="avatar-text">{{ avatarText }}</text>
          </view>
        </view>
        <text class="profile-name">{{ displayName }}</text>
      </template>
    </view>

    <view v-if="isAuthenticated" class="entry-grid">
      <view class="surface-card entry-card" @click="goToEdit">
        <view class="entry-icon entry-icon--edit">
          <view class="icon-sheet"></view>
          <view class="icon-pencil"></view>
        </view>
        <text class="entry-name">编辑资料</text>
      </view>

      <view class="surface-card entry-card" @click="confirmLogout">
        <view class="entry-icon entry-icon--logout">
          <view class="icon-door"></view>
          <view class="icon-arrow"></view>
        </view>
        <text class="entry-name">退出登录</text>
      </view>
    </view>
  </view>
</template>

<script setup lang="ts">
import { onShow } from '@dcloudio/uni-app'
import { computed } from 'vue'

import { useCustomerSession } from '@/features/customer-account/composables/useCustomerSession'
import { resolveAvatarUrl } from '@/features/customer-account/utils/avatar'

const { state, isAuthenticated, restoreSession, logout } = useCustomerSession()

const avatarImageUrl = computed(() => resolveAvatarUrl(state.currentCustomer?.avatarUrl))
const showLoginHint = computed(() => !isAuthenticated.value && !state.isRestoring)
const displayName = computed(() => {
  const nickName = state.currentCustomer?.nickName?.trim()
  if (nickName) return nickName
  return state.isLoadingCustomer ? '正在同步资料' : '未设置昵称'
})

const avatarText = computed(() => {
  const name = state.currentCustomer?.nickName?.trim()
  if (name) return name.slice(0, 1).toUpperCase()
  return isAuthenticated.value ? '会' : '我'
})

void initializeProfile()

onShow(() => {
  void initializeProfile()
})

async function initializeProfile() {
  try {
    await restoreSession()
  } catch (error) {
    showToast(error)
  }
}

function goToLogin() {
  uni.navigateTo({ url: '/features/customer-account/pages/login/index' })
}

function goToEdit() {
  uni.navigateTo({ url: '/features/customer-account/pages/edit/index' })
}

function confirmLogout() {
  uni.showModal({
    title: '退出登录',
    content: '确认退出当前账户吗？',
    success: async (result) => {
      if (!result.confirm) return
      try {
        await logout()
        uni.showToast({ title: '已退出登录', icon: 'success' })
      } catch (error) {
        showToast(error)
      }
    },
  })
}

function showToast(error: unknown) {
  const message = error instanceof Error ? error.message : '操作失败，请稍后重试'
  uni.showToast({ title: message, icon: 'none' })
}
</script>

<style scoped>
.profile-page {
  display: flex;
  flex-direction: column;
  gap: 18rpx;
}

.header-card {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 14rpx;
  padding: 28rpx 20rpx 24rpx;
  overflow: hidden;
}

.header-card--guest {
  align-items: stretch;
  padding: 0;
  background:
    radial-gradient(circle at top right, rgba(47, 107, 98, 0.2) 0%, rgba(47, 107, 98, 0) 36%),
    linear-gradient(135deg, rgba(250, 253, 252, 0.98) 0%, rgba(238, 245, 243, 0.98) 100%);
}

.guest-hero {
  display: flex;
  flex-direction: column;
  gap: 20rpx;
  padding: 30rpx 26rpx 28rpx;
}

.guest-copy {
  display: flex;
  flex-direction: column;
  gap: 0;
  justify-content: center;
  min-height: 116rpx;
}

.avatar-shell {
  width: 108rpx;
  height: 108rpx;
  padding: 5rpx;
  border-radius: 999rpx;
  background: linear-gradient(135deg, rgba(47, 107, 98, 0.2) 0%, rgba(32, 58, 55, 0.08) 100%);
}

.avatar-shell--guest {
  position: absolute;
  top: 24rpx;
  right: 24rpx;
  width: 116rpx;
  height: 116rpx;
  padding: 6rpx;
  background: linear-gradient(135deg, rgba(47, 107, 98, 0.26) 0%, rgba(255, 255, 255, 0.9) 100%);
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

.avatar-fallback--guest {
  background: linear-gradient(180deg, #fcfefd 0%, #e8f3f0 100%);
}

.avatar-text {
  font-size: 34rpx;
  font-weight: 700;
  color: var(--brand-accent);
}

.profile-name {
  font-size: 28rpx;
  font-weight: 600;
  color: var(--text-primary);
}

.profile-name--guest {
  max-width: 440rpx;
  padding-right: 120rpx;
  line-height: 1.4;
}

.login-action {
  margin-top: 6rpx;
}

.entry-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16rpx;
}

.entry-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 18rpx;
  min-height: 188rpx;
  padding: 22rpx 14rpx;
}

.entry-name {
  font-size: 25rpx;
  font-weight: 600;
  color: var(--text-primary);
}

.entry-icon {
  position: relative;
  width: 56rpx;
  height: 56rpx;
  border-radius: 18rpx;
  background: rgba(47, 107, 98, 0.1);
}

.entry-icon--edit .icon-sheet {
  position: absolute;
  left: 14rpx;
  top: 12rpx;
  width: 24rpx;
  height: 30rpx;
  border: 3rpx solid var(--brand-accent);
  border-radius: 8rpx;
}

.entry-icon--edit .icon-pencil {
  position: absolute;
  right: 11rpx;
  bottom: 13rpx;
  width: 18rpx;
  height: 3rpx;
  background: var(--brand-accent);
  transform: rotate(-45deg);
  transform-origin: right center;
}

.entry-icon--logout .icon-door {
  position: absolute;
  left: 14rpx;
  top: 11rpx;
  width: 18rpx;
  height: 32rpx;
  border: 3rpx solid var(--brand-accent);
  border-radius: 6rpx;
}

.entry-icon--logout .icon-arrow {
  position: absolute;
  right: 11rpx;
  top: 26rpx;
  width: 18rpx;
  height: 3rpx;
  background: var(--brand-accent);
}

.entry-icon--logout .icon-arrow::after {
  content: '';
  position: absolute;
  right: -1rpx;
  top: -5rpx;
  width: 9rpx;
  height: 9rpx;
  border-right: 3rpx solid var(--brand-accent);
  border-top: 3rpx solid var(--brand-accent);
  transform: rotate(45deg);
}
</style>
