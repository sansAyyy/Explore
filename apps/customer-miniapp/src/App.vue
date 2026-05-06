<script setup lang="ts">
import { onLaunch } from '@dcloudio/uni-app'

import { useCustomerSession } from '@/features/customer-account/composables/useCustomerSession'

const { restoreSession } = useCustomerSession()

onLaunch(() => {
  void restoreSession()
})
</script>

<style>
:root,
page {
  --page-bg-start: #eef5f3;
  --page-bg-end: #f8fbfa;
  --surface-card: rgba(255, 255, 255, 0.94);
  --surface-soft: rgba(255, 255, 255, 0.78);
  --surface-muted: rgba(239, 245, 243, 0.9);
  --text-primary: #1f2a28;
  --text-secondary: #47605b;
  --text-muted: #6d817d;
  --brand-primary: #203a37;
  --brand-accent: #2f6b62;
  --brand-accent-soft: rgba(47, 107, 98, 0.14);
  --border-soft: rgba(32, 58, 55, 0.08);
  --border-strong: rgba(32, 58, 55, 0.14);
  --danger-text: #8f2d23;
  --shadow-soft: 0 18rpx 40rpx rgba(32, 58, 55, 0.06);
}

page {
  background: linear-gradient(180deg, var(--page-bg-start) 0%, var(--page-bg-end) 100%);
  color: var(--text-primary);
  font-size: 26rpx;
}

view,
text,
button,
input,
image {
  box-sizing: border-box;
}

button {
  border-radius: 20rpx;
  font-size: 26rpx;
}

button::after {
  border: none;
}

.page-shell {
  min-height: 100vh;
  padding: 24rpx 20rpx 40rpx;
}

.surface-card {
  background: var(--surface-card);
  border: 2rpx solid var(--border-soft);
  border-radius: 28rpx;
  box-shadow: var(--shadow-soft);
}

.section-heading {
  display: flex;
  flex-direction: column;
  gap: 8rpx;
  margin-bottom: 20rpx;
}

.section-title {
  font-size: 32rpx;
  font-weight: 600;
  color: var(--text-primary);
}

.section-caption {
  color: var(--text-muted);
  line-height: 1.6;
}

.primary-action,
.secondary-action,
.ghost-action {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 76rpx;
  padding: 0 22rpx;
  border-radius: 18rpx;
  font-size: 26rpx;
  font-weight: 600;
}

.primary-action {
  background: linear-gradient(135deg, #203a37 0%, #2f6b62 100%);
  color: #ffffff;
  box-shadow: 0 14rpx 24rpx rgba(32, 58, 55, 0.16);
}

.secondary-action {
  background: rgba(255, 255, 255, 0.92);
  color: var(--brand-primary);
  border: 2rpx solid var(--border-strong);
}

.ghost-action {
  background: transparent;
  color: var(--text-secondary);
  border: 2rpx solid rgba(109, 129, 125, 0.2);
}

.primary-action[disabled],
.secondary-action[disabled],
.ghost-action[disabled] {
  opacity: 0.6;
}

.form-input {
  width: 100%;
  min-height: 76rpx;
  padding: 0 20rpx;
  border: 2rpx solid rgba(32, 58, 55, 0.1);
  border-radius: 18rpx;
  background: #ffffff;
  color: var(--text-primary);
}

.status-pill {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 48rpx;
  padding: 0 18rpx;
  border-radius: 999rpx;
  font-size: 24rpx;
  font-weight: 600;
}

.muted-panel {
  background: var(--surface-muted);
  border: 2rpx solid rgba(32, 58, 55, 0.06);
  border-radius: 20rpx;
}
</style>
