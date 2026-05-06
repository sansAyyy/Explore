<script setup lang="ts">
import type {
  ChangeCurrentAdminPasswordRequest,
  UpdateCurrentAdminProfileRequest,
} from '@/features/current-admin/types/currentAdmin'

import { Lock, RefreshRight, UserFilled } from '@element-plus/icons-vue'
import { computed, onMounted, ref } from 'vue'

import CurrentAdminPasswordDrawer from '@/features/current-admin/components/drawers/CurrentAdminPasswordDrawer.vue'
import CurrentAdminProfileEditDrawer from '@/features/current-admin/components/drawers/CurrentAdminProfileEditDrawer.vue'
import { CURRENT_ADMIN_MIN_PASSWORD_LENGTH } from '@/features/current-admin/models/currentAdminPasswordForm'
import { useCurrentAdminProfile } from '@/features/current-admin/composables/useCurrentAdminProfile'
import { formatDateTime } from '@/shared/utils/dateTime'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const {
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
} = useCurrentAdminProfile()

const isFirstLoadSettled = ref(false)
const isPasswordDrawerOpen = ref(false)
const isProfileDrawerOpen = ref(false)

const roleCodes = computed(() => authStore.roleCodes.filter(Boolean))
const roleLabel = computed(() => roleCodes.value.join(' / ') || '未分配角色')
const roleCountLabel = computed(() => `${roleCodes.value.length} 个角色`)
const identityTitle = computed(
  () => currentAdmin.value?.displayName || authStore.displayName || authStore.userName || '个人中心',
)
const loginName = computed(() => currentAdmin.value?.userName || authStore.userName || '-')
const phoneNumber = computed(() => currentAdmin.value?.phoneNumber || '未填写')
const createdAtLabel = computed(() => formatDateTime(currentAdmin.value?.createdAt))
const updatedAtLabel = computed(
  () => formatDateTime(currentAdmin.value?.updatedAt, { fallback: '暂无记录' }),
)
const lastLoginAtLabel = computed(
  () => formatDateTime(currentAdmin.value?.lastLoginAt, { fallback: '暂无记录' }),
)
const syncedAtLabel = computed(() => formatDateTime(lastLoadedAt.value, { fallback: '尚未同步' }))

const statusItems = computed(() => [
  {
    label: '账号状态',
    value: currentAdmin.value?.isActive ? '已启用' : '已停用',
    tone: currentAdmin.value?.isActive ? 'success' : 'warning',
  },
  {
    label: '角色数量',
    value: roleCountLabel.value,
    tone: 'default',
  },
  {
    label: '最近同步',
    value: syncedAtLabel.value,
    tone: 'default',
  },
])

const overviewItems = computed(() => [
  {
    label: '邮箱',
    value: currentAdmin.value?.email || '-',
  },
  {
    label: '手机号',
    value: phoneNumber.value,
  },
  {
    label: '最近登录',
    value: lastLoginAtLabel.value,
  },
  {
    label: '最近更新',
    value: updatedAtLabel.value,
  },
  {
    label: '资料版本',
    value: currentAdmin.value ? `V${currentAdmin.value.version}` : '-',
  },
  {
    label: '创建时间',
    value: createdAtLabel.value,
  },
])

const actionItems = computed(() => [
  '账号、邮箱、显示名称通过抽屉维护',
  '手机号在个人中心保持只读',
  `新密码长度不少于 ${CURRENT_ADMIN_MIN_PASSWORD_LENGTH} 位`,
])

const showInitialLoading = computed(() => !isFirstLoadSettled.value || (isLoading.value && !currentAdmin.value))
const showLoadError = computed(() =>
  hasLoaded.value && isFirstLoadSettled.value && !isLoading.value && !currentAdmin.value && Boolean(loadErrorMessage.value),
)
const hasRefreshWarning = computed(() => Boolean(loadErrorMessage.value) && Boolean(currentAdmin.value))
const overviewStatusText = computed(() => {
  if (showLoadError.value) {
    return loadErrorMessage.value
  }

  if (hasRefreshWarning.value) {
    return '最近一次刷新失败，当前展示最近一次成功同步的数据。'
  }

  if (isLoading.value) {
    return '正在同步当前管理员资料。'
  }

  if (lastLoadedAt.value) {
    return `已同步于 ${syncedAtLabel.value}`
  }

  return '当前账号摘要'
})

const canOpenActions = computed(() => Boolean(currentAdmin.value) && !showInitialLoading.value)

async function handleRefresh() {
  try {
    await loadCurrentAdmin()
  } catch {
    // Errors are surfaced through ElMessage in the composable and inline state here.
  }
}

async function handleProfileSubmit(payload: UpdateCurrentAdminProfileRequest) {
  try {
    await saveProfile(payload)
    isProfileDrawerOpen.value = false
  } catch {
    // Errors are surfaced through ElMessage in the composable.
  }
}

async function handlePasswordSubmit(payload: ChangeCurrentAdminPasswordRequest) {
  try {
    await updatePassword(payload)
    isPasswordDrawerOpen.value = false
  } catch {
    // Errors are surfaced through ElMessage in the composable.
  }
}

onMounted(async () => {
  await handleRefresh()
  isFirstLoadSettled.value = true
})
</script>

<template>
  <main class="profile-page">
    <section
      v-if="showLoadError"
      class="profile-page__empty-state"
      aria-live="polite"
    >
      <p class="profile-page__section-label">同步失败</p>
      <h1>无法加载个人资料</h1>
      <p>{{ loadErrorMessage }}</p>
      <el-button
        type="primary"
        :icon="RefreshRight"
        :loading="isLoading"
        @click="handleRefresh"
      >
        重新加载
      </el-button>
    </section>

    <template v-else>
      <section class="profile-page__hero-card">
        <header class="profile-page__hero-header">
          <div class="profile-page__hero-copy">
            <p class="profile-page__section-label">个人中心</p>
            <h1 class="profile-page__title">{{ identityTitle }}</h1>
            <p class="profile-page__subtitle">{{ loginName }} · {{ roleLabel }}</p>
          </div>

          <div class="profile-page__toolbar">
            <el-button
              :icon="RefreshRight"
              :loading="isLoading"
              @click="handleRefresh"
            >
              {{ isLoading ? '同步中' : '刷新' }}
            </el-button>
            <el-button
              plain
              :disabled="!canOpenActions"
              @click="isProfileDrawerOpen = true"
            >
              修改资料
            </el-button>
            <el-button
              type="primary"
              :icon="Lock"
              :disabled="!canOpenActions"
              @click="isPasswordDrawerOpen = true"
            >
              修改密码
            </el-button>
          </div>
        </header>

        <div class="profile-page__hero-body">
          <div class="profile-page__identity-card">
            <div class="profile-page__identity-header">
              <div class="profile-page__avatar" aria-hidden="true">
                <el-icon><UserFilled /></el-icon>
              </div>

              <div class="profile-page__identity-meta">
                <strong>{{ loginName }}</strong>
                <span>{{ roleLabel }}</span>
              </div>
            </div>

            <div class="profile-page__identity-brief">
              <div>
                <small>最近登录</small>
                <strong>{{ lastLoginAtLabel }}</strong>
              </div>
              <div>
                <small>最近更新</small>
                <strong>{{ updatedAtLabel }}</strong>
              </div>
            </div>
          </div>

          <dl class="profile-page__status-grid">
            <div
              v-for="item in statusItems"
              :key="item.label"
              class="profile-page__status-card"
              :class="`profile-page__status-card--${item.tone}`"
            >
              <dt>{{ item.label }}</dt>
              <dd>{{ item.value }}</dd>
            </div>
          </dl>
        </div>
      </section>

      <section class="profile-page__board">
        <article class="profile-page__panel">
          <header class="profile-page__panel-header">
            <div>
              <p class="profile-page__section-label">账户摘要</p>
              <h2 class="profile-page__panel-title">基础信息</h2>
            </div>
            <span class="profile-page__panel-meta">{{ overviewStatusText }}</span>
          </header>

          <el-skeleton :loading="showInitialLoading" animated :rows="4">
            <dl class="profile-page__overview-grid">
              <div
                v-for="item in overviewItems"
                :key="item.label"
                class="profile-page__overview-item"
              >
                <dt>{{ item.label }}</dt>
                <dd>{{ item.value }}</dd>
              </div>
            </dl>
          </el-skeleton>
        </article>

        <article class="profile-page__panel profile-page__panel--compact">
          <header class="profile-page__panel-header">
            <div>
              <p class="profile-page__section-label">常用操作</p>
              <h2 class="profile-page__panel-title">维护入口</h2>
            </div>
          </header>

          <div class="profile-page__action-stack">
            <el-button
              type="primary"
              plain
              :disabled="!canOpenActions"
              @click="isProfileDrawerOpen = true"
            >
              修改个人资料
            </el-button>
            <el-button
              type="primary"
              :disabled="!canOpenActions"
              @click="isPasswordDrawerOpen = true"
            >
              更新登录密码
            </el-button>
          </div>

          <ul class="profile-page__bullet-list">
            <li v-for="item in actionItems" :key="item">{{ item }}</li>
          </ul>
        </article>
      </section>
    </template>

    <CurrentAdminProfileEditDrawer
      v-model="isProfileDrawerOpen"
      :is-submitting="isProfileSubmitting"
      :profile="currentAdmin"
      @submit="handleProfileSubmit"
    />

    <CurrentAdminPasswordDrawer
      v-model="isPasswordDrawerOpen"
      :is-submitting="isPasswordSubmitting"
      @submit="handlePasswordSubmit"
    />
  </main>
</template>

<style scoped>
.profile-page {
  --profile-primary: #0369a1;
  --profile-success: #22c55e;
  --profile-warning: #f97316;
  --profile-text: #0c4a6e;
  --profile-text-strong: #082f49;
  --profile-muted: #64748b;
  --profile-panel-border: rgba(148, 163, 184, 0.22);
  --profile-panel: rgba(255, 255, 255, 0.96);
  display: grid;
  grid-template-rows: auto minmax(0, 1fr);
  gap: 10px;
  height: 100%;
  min-height: 0;
  color: var(--profile-text);
  overflow: hidden;
}

.profile-page__hero-card,
.profile-page__panel,
.profile-page__empty-state {
  border: 1px solid var(--profile-panel-border);
  border-radius: 20px;
  background: var(--profile-panel);
  box-shadow:
    0 12px 30px rgba(8, 47, 73, 0.07),
    0 4px 12px rgba(14, 165, 233, 0.04);
}

.profile-page__hero-card {
  display: grid;
  gap: 16px;
  padding: 18px 20px;
  background:
    radial-gradient(circle at top right, rgba(14, 165, 233, 0.14), transparent 34%),
    linear-gradient(180deg, rgba(240, 249, 255, 0.94), rgba(255, 255, 255, 0.98));
}

.profile-page__hero-header,
.profile-page__panel-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.profile-page__hero-copy {
  display: grid;
  gap: 4px;
  min-width: 0;
}

.profile-page__section-label {
  margin: 0;
  color: var(--profile-primary);
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

.profile-page__title {
  margin: 0;
  color: var(--profile-text-strong);
  font-size: clamp(22px, 2.2vw, 28px);
  font-weight: 760;
  line-height: 1.05;
  letter-spacing: -0.04em;
}

.profile-page__subtitle,
.profile-page__panel-meta,
.profile-page__empty-state p {
  margin: 0;
  color: var(--profile-muted);
  font-size: 12px;
  line-height: 1.6;
}

.profile-page__toolbar {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 10px;
}

.profile-page__toolbar :deep(.el-button),
.profile-page__action-stack :deep(.el-button),
.profile-page__empty-state :deep(.el-button) {
  min-height: 40px;
  border-radius: 14px;
  font-weight: 600;
}

.profile-page__toolbar :deep(.el-button + .el-button),
.profile-page__action-stack :deep(.el-button + .el-button) {
  margin-left: 0;
}

.profile-page__hero-body {
  display: grid;
  grid-template-columns: minmax(280px, 360px) minmax(0, 1fr);
  gap: 14px;
}

.profile-page__identity-card {
  display: grid;
  gap: 12px;
  padding: 14px;
  border: 1px solid rgba(125, 211, 252, 0.32);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.84);
}

.profile-page__identity-header {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.profile-page__avatar {
  display: inline-grid;
  flex: none;
  place-items: center;
  width: 56px;
  height: 56px;
  border-radius: 18px;
  background: linear-gradient(135deg, rgba(3, 105, 161, 0.92), rgba(14, 165, 233, 0.72));
  color: #ffffff;
  font-size: 24px;
}

.profile-page__identity-meta {
  display: grid;
  gap: 4px;
  min-width: 0;
}

.profile-page__identity-meta strong {
  color: var(--profile-text-strong);
  font-size: 16px;
  font-weight: 700;
  line-height: 1.2;
  word-break: break-word;
}

.profile-page__identity-meta span {
  color: var(--profile-muted);
  font-size: 12px;
  line-height: 1.5;
}

.profile-page__identity-brief {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
}

.profile-page__identity-brief div,
.profile-page__overview-item,
.profile-page__status-card {
  padding: 12px 14px;
  border-radius: 16px;
  background: rgba(248, 250, 252, 0.88);
  box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.16);
}

.profile-page__identity-brief small,
.profile-page__overview-item dt,
.profile-page__status-card dt {
  display: block;
  margin: 0 0 6px;
  color: var(--profile-muted);
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.profile-page__identity-brief strong,
.profile-page__overview-item dd,
.profile-page__status-card dd {
  margin: 0;
  color: var(--profile-text-strong);
  font-size: 13px;
  font-weight: 600;
  line-height: 1.45;
  word-break: break-word;
}

.profile-page__status-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 10px;
  margin: 0;
}

.profile-page__status-card--success {
  background: linear-gradient(180deg, rgba(255, 255, 255, 0.96), rgba(240, 253, 244, 0.98));
}

.profile-page__status-card--warning {
  background: linear-gradient(180deg, rgba(255, 255, 255, 0.96), rgba(255, 247, 237, 0.98));
}

.profile-page__board {
  display: grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(300px, 0.9fr);
  gap: 10px;
  min-height: 0;
}

.profile-page__panel {
  display: grid;
  align-content: start;
  gap: 14px;
  padding: 18px;
  min-height: 0;
}

.profile-page__panel-title {
  margin: 4px 0 0;
  color: var(--profile-text-strong);
  font-size: 18px;
  font-weight: 720;
  line-height: 1.15;
  letter-spacing: -0.02em;
}

.profile-page__overview-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 10px;
  margin: 0;
}

.profile-page__panel--compact {
  gap: 16px;
}

.profile-page__action-stack {
  display: grid;
  gap: 10px;
}

.profile-page__action-stack :deep(.el-button) {
  width: 100%;
}

.profile-page__bullet-list {
  display: grid;
  gap: 10px;
  margin: 0;
  padding: 0;
  list-style: none;
}

.profile-page__bullet-list li {
  padding-left: 16px;
  color: var(--profile-muted);
  font-size: 12px;
  line-height: 1.6;
  position: relative;
}

.profile-page__bullet-list li::before {
  position: absolute;
  top: 7px;
  left: 0;
  width: 6px;
  height: 6px;
  border-radius: 999px;
  background: var(--profile-primary);
  content: '';
}

.profile-page__empty-state {
  display: grid;
  align-content: center;
  justify-items: start;
  gap: 10px;
  height: 100%;
  padding: 28px;
}

.profile-page__empty-state h1 {
  margin: 0;
  color: var(--profile-text-strong);
  font-size: 24px;
  font-weight: 740;
  line-height: 1.1;
}

@media (max-width: 1180px) {
  .profile-page {
    grid-template-rows: auto auto;
    overflow: auto;
  }

  .profile-page__hero-body,
  .profile-page__board,
  .profile-page__overview-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 860px) {
  .profile-page__hero-header,
  .profile-page__panel-header {
    flex-direction: column;
    align-items: stretch;
  }

  .profile-page__toolbar {
    justify-content: stretch;
  }

  .profile-page__toolbar :deep(.el-button) {
    flex: 1 1 0;
  }

  .profile-page__status-grid,
  .profile-page__identity-brief {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .profile-page {
    gap: 10px;
  }

  .profile-page__hero-card,
  .profile-page__panel,
  .profile-page__empty-state {
    padding: 16px;
  }

  .profile-page__title {
    font-size: clamp(22px, 8vw, 26px);
  }

  .profile-page__toolbar,
  .profile-page__toolbar :deep(.el-button) {
    width: 100%;
  }
}

@media (prefers-reduced-motion: reduce) {
  .profile-page *,
  .profile-page *::before,
  .profile-page *::after {
    animation: none !important;
    transition: none !important;
    scroll-behavior: auto !important;
  }
}
</style>
