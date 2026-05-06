<script setup lang="ts">
import { ArrowDown, Bell, Expand, Fold, Menu, SwitchButton, UserFilled } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

defineProps<{
  collapsed: boolean
  displayName: string
  isLoggingOut: boolean
  isSyncingSession: boolean
  pageTitle: string
  roleLabel: string
}>()

defineEmits<{
  logout: []
  'navigate-profile': []
  'open-sidebar': []
  'toggle-sidebar': []
}>()

function handleInboxClick() {
  ElMessage.info('站内信功能即将上线')
}
</script>

<template>
  <header class="admin-topbar">
    <div class="admin-topbar__primary">
      <el-button
        :icon="Menu"
        circle
        class="admin-topbar__mobile-trigger"
        @click="$emit('open-sidebar')"
      />

      <el-button
        :icon="collapsed ? Expand : Fold"
        circle
        class="admin-topbar__desktop-trigger"
        @click="$emit('toggle-sidebar')"
      />

      <div class="admin-topbar__title-group">
        <p class="admin-topbar__eyebrow">Explore Admin</p>
        <strong>{{ pageTitle }}</strong>
      </div>
    </div>

    <div class="admin-topbar__actions">
      <el-badge is-dot class="admin-topbar__notice-badge">
        <el-button
          :icon="Bell"
          circle
          class="admin-topbar__notice-trigger"
          @click="handleInboxClick"
        />
      </el-badge>

      <el-dropdown
        class="admin-topbar__account-dropdown"
        placement="bottom-end"
        trigger="click"
      >
        <button class="admin-topbar__account-trigger" type="button">
          <span class="admin-topbar__avatar">
            <el-icon><UserFilled /></el-icon>
          </span>

          <span class="admin-topbar__account-copy">
            <span class="admin-topbar__user-name">{{ displayName }}</span>
            <small>{{ roleLabel }}</small>
          </span>

          <el-icon class="admin-topbar__account-arrow">
            <ArrowDown />
          </el-icon>
        </button>

        <template #dropdown>
          <el-dropdown-menu class="admin-topbar__menu">
            <div class="admin-topbar__menu-profile">
              <strong>{{ displayName }}</strong>
              <span>{{ roleLabel }}</span>
            </div>

            <el-dropdown-item :icon="UserFilled" @click="$emit('navigate-profile')">
              个人中心
            </el-dropdown-item>

            <el-dropdown-item divided :icon="SwitchButton" @click="$emit('logout')">
              退出登录
            </el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </div>

    <Transition name="admin-topbar-progress">
      <div
        v-if="isSyncingSession"
        class="admin-topbar__progress"
        role="status"
        aria-live="polite"
        aria-label="正在同步登录状态"
      >
        <span class="admin-topbar__progress-bar" />
      </div>
    </Transition>
  </header>
</template>

<style scoped>
.admin-topbar {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 12px 18px;
  border-bottom: 1px solid rgba(111, 123, 146, 0.16);
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px);
  overflow: hidden;
}

.admin-topbar__primary {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.admin-topbar__mobile-trigger,
.admin-topbar__desktop-trigger,
.admin-topbar__notice-trigger {
  flex: none;
  border-color: rgba(137, 152, 178, 0.24);
  background: rgba(245, 247, 252, 0.94);
  color: #31435f;
}

.admin-topbar__mobile-trigger {
  display: none;
}

.admin-topbar__title-group {
  min-width: 0;
}

.admin-topbar__eyebrow {
  margin: 0 0 4px;
  color: #6b7890;
  font-size: 11px;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.admin-topbar__title-group strong {
  display: block;
  color: #132746;
  font-size: clamp(20px, 2.2vw, 24px);
  line-height: 1.08;
}

.admin-topbar__actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.admin-topbar__notice-badge :deep(.el-badge__content.is-fixed.is-dot) {
  top: 8px;
  right: 8px;
}

.admin-topbar__account-trigger {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
  padding: 6px 10px 6px 8px;
  border: 1px solid rgba(137, 152, 178, 0.18);
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.92);
  color: #152844;
  cursor: pointer;
  transition: border-color 0.2s ease, box-shadow 0.2s ease, background-color 0.2s ease;
}

.admin-topbar__account-trigger:hover {
  border-color: rgba(96, 125, 182, 0.28);
  background: #ffffff;
  box-shadow: 0 10px 24px rgba(26, 42, 76, 0.08);
}

.admin-topbar__avatar {
  display: inline-grid;
  place-items: center;
  width: 34px;
  height: 34px;
  flex: none;
  border-radius: 50%;
  background: linear-gradient(135deg, #194ebb 0%, #4b84ff 100%);
  color: #ffffff;
  font-size: 14px;
  box-shadow: 0 10px 20px rgba(25, 78, 187, 0.22);
}

.admin-topbar__account-copy {
  display: grid;
  min-width: 0;
  text-align: left;
}

.admin-topbar__user-name {
  color: #152844;
  font-weight: 700;
}

.admin-topbar__account-copy small {
  color: #6b7890;
  font-size: 12px;
}

.admin-topbar__account-arrow {
  color: #6b7890;
  font-size: 14px;
}

.admin-topbar__menu :deep(.el-dropdown-menu__item) {
  min-width: 180px;
}

.admin-topbar__menu-profile {
  display: grid;
  gap: 4px;
  padding: 6px 14px 10px;
}

.admin-topbar__menu-profile strong {
  color: #152844;
}

.admin-topbar__menu-profile span {
  color: #6b7890;
  font-size: 12px;
}

.admin-topbar__progress {
  position: absolute;
  right: 0;
  bottom: 0;
  left: 0;
  height: 2px;
  background: rgba(59, 130, 246, 0.08);
}

.admin-topbar__progress-bar {
  display: block;
  width: 32%;
  height: 100%;
  border-radius: 999px;
  background: linear-gradient(90deg, #3b82f6 0%, #60a5fa 55%, #93c5fd 100%);
  animation: admin-topbar-progress-slide 1.15s ease-in-out infinite;
  will-change: transform;
}

.admin-topbar-progress-enter-active,
.admin-topbar-progress-leave-active {
  transition: opacity 0.18s ease;
}

.admin-topbar-progress-enter-from,
.admin-topbar-progress-leave-to {
  opacity: 0;
}

@keyframes admin-topbar-progress-slide {
  0% {
    transform: translateX(-120%);
  }

  100% {
    transform: translateX(420%);
  }
}

@media (max-width: 920px) {
  .admin-topbar {
    padding: 16px;
  }

  .admin-topbar__mobile-trigger {
    display: inline-flex;
  }

  .admin-topbar__desktop-trigger {
    display: none;
  }
}

@media (max-width: 640px) {
  .admin-topbar {
    gap: 12px;
    padding: 14px 12px;
  }

  .admin-topbar__account-copy,
  .admin-topbar__account-arrow {
    display: none;
  }

  .admin-topbar__account-trigger {
    padding-right: 10px;
  }
}
</style>
