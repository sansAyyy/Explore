<template>
  <view class="page-shell home-page">
    <view class="surface-card top-card">
      <text class="top-title">服务</text>
    </view>
  </view>
</template>

<script setup lang="ts">
import { onShow } from '@dcloudio/uni-app'
import { computed } from 'vue'

import { useCustomerSession } from '@/features/customer-account/composables/useCustomerSession'

const { isAuthenticated, restoreSession } = useCustomerSession()


onShow(() => {
  void restoreSession()
})
</script>

<style scoped>
.home-page {
  display: flex;
  flex-direction: column;
  gap: 20rpx;
}

.top-card {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  padding: 28rpx 24rpx;
}

.top-title {
  font-size: 34rpx;
  font-weight: 600;
  color: var(--text-primary);
}
</style>
