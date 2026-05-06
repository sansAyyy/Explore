<script setup lang="ts">
import { computed } from 'vue'

import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

const summaryItems = computed(() => [
  {
    label: '角色数量',
    value: authStore.roleCodes.length,
  },
  {
    label: '全部权限',
    value: authStore.permissionCodes.length,
  },
  {
    label: '页面权限',
    value: authStore.pagePermissionCodes.length,
  },
  {
    label: '按钮权限',
    value: authStore.buttonPermissionCodes.length,
  },
])

const visiblePermissionCodes = computed(() => authStore.pagePermissionCodes.slice(0, 6))
</script>

<template>
  <div class="dashboard-page">
    <section class="dashboard-page__hero">
      <div class="dashboard-page__hero-main">
        <p class="dashboard-page__eyebrow">Dashboard</p>
        <h1 class="dashboard-page__title">{{ authStore.displayName || authStore.userName || '管理员' }}</h1>
        <p class="dashboard-page__lead">
          当前页作为后台工作台概览，集中展示登录身份、权限上下文和核心授权指标，方便快速确认 RBAC 集成状态。
        </p>
      </div>

      <el-card shadow="never" class="dashboard-page__profile-card">
        <div class="dashboard-page__profile-header">
          <div class="dashboard-page__card-title">当前会话</div>
          <div class="dashboard-page__card-meta">已登录身份与令牌上下文</div>
        </div>

        <el-descriptions :column="1" border class="dashboard-page__descriptions">
          <el-descriptions-item label="用户 ID">
            {{ authStore.userId || '-' }}
          </el-descriptions-item>
          <el-descriptions-item label="账号">
            {{ authStore.userName || '-' }}
          </el-descriptions-item>
          <el-descriptions-item label="Token 类型">
            {{ authStore.tokenType || '-' }}
          </el-descriptions-item>
        </el-descriptions>
      </el-card>
    </section>

    <section class="dashboard-page__summary">
      <el-card v-for="item in summaryItems" :key="item.label" shadow="never" class="dashboard-page__summary-card">
        <p class="dashboard-page__summary-label">{{ item.label }}</p>
        <strong class="dashboard-page__summary-value">{{ item.value }}</strong>
      </el-card>
    </section>

    <section class="dashboard-page__detail">
      <el-card shadow="never" class="dashboard-page__detail-card">
        <template #header>
          <div class="dashboard-page__detail-header">
            <div>
              <div class="dashboard-page__card-title">页面权限预览</div>
              <p class="dashboard-page__card-meta">展示当前账号可访问的部分页面权限码</p>
            </div>
          </div>
        </template>

        <div class="dashboard-page__tag-list">
          <el-tag v-for="code in visiblePermissionCodes" :key="code" type="primary" effect="plain">
            {{ code }}
          </el-tag>
          <span v-if="visiblePermissionCodes.length === 0" class="dashboard-page__empty">
            当前没有页面权限。
          </span>
        </div>
      </el-card>

      <el-card shadow="never" class="dashboard-page__detail-card">
        <template #header>
          <div class="dashboard-page__detail-header">
            <div>
              <div class="dashboard-page__card-title">按钮权限示例</div>
              <p class="dashboard-page__card-meta">用实际控件验证显隐控制已生效</p>
            </div>
          </div>
        </template>

        <div class="dashboard-page__button-row">
          <el-button v-if="authStore.hasButtonPermission('admin_users.create')" type="primary">
            创建管理员
          </el-button>
          <el-button
            v-if="authStore.hasButtonPermission('admin_users.assign_roles')"
            type="success"
            plain
          >
            分配角色
          </el-button>
          <span
            v-if="
              !authStore.hasButtonPermission('admin_users.create') &&
              !authStore.hasButtonPermission('admin_users.assign_roles')
            "
            class="dashboard-page__empty"
          >
            当前账号没有示例按钮权限，说明显隐控制已经生效。
          </span>
        </div>
      </el-card>
    </section>
  </div>
</template>

<style scoped>
.dashboard-page {
  display: grid;
  gap: 10px;
  min-height: 0;
  color: #334155;
}

.dashboard-page__hero {
  display: grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(300px, 360px);
  gap: 10px;
  align-items: stretch;
}

.dashboard-page__hero-main,
.dashboard-page__profile-card,
.dashboard-page__summary-card,
.dashboard-page__detail-card {
  border: 1px solid rgba(148, 163, 184, 0.24);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.96);
  box-shadow:
    0 12px 24px rgba(15, 23, 42, 0.04),
    0 4px 10px rgba(59, 130, 246, 0.05);
  backdrop-filter: blur(10px);
}

.dashboard-page__hero-main {
  display: grid;
  align-content: start;
  gap: 6px;
  min-width: 0;
  padding: 18px 20px;
  background:
    radial-gradient(circle at top right, rgba(59, 130, 246, 0.12), transparent 34%),
    linear-gradient(180deg, rgba(248, 250, 252, 0.9), rgba(255, 255, 255, 0.98));
}

.dashboard-page__eyebrow {
  margin: 0;
  color: #64748b;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.dashboard-page__title {
  margin: 0;
  color: #0f172a;
  font-size: clamp(24px, 2.2vw, 30px);
  font-weight: 760;
  line-height: 1.02;
  letter-spacing: -0.04em;
}

.dashboard-page__lead {
  max-width: 760px;
  margin: 2px 0 0;
  color: #64748b;
  font-size: 13px;
  line-height: 1.55;
}

.dashboard-page__profile-card {
  padding: 16px;
}

.dashboard-page__profile-header,
.dashboard-page__detail-header {
  padding-bottom: 4px;
}

.dashboard-page__card-title {
  color: #0f172a;
  font-size: 13px;
  font-weight: 700;
  line-height: 1.25;
}

.dashboard-page__card-meta {
  margin: 2px 0 0;
  color: #64748b;
  font-size: 11px;
  line-height: 1.4;
}

.dashboard-page__descriptions :deep(.el-descriptions__body) {
  margin-top: 10px;
  border-radius: 14px;
  overflow: hidden;
}

.dashboard-page__descriptions :deep(.el-descriptions__label) {
  width: 100px;
  color: #334155;
  font-weight: 600;
  background: rgba(248, 250, 252, 0.92);
}

.dashboard-page__descriptions :deep(.el-descriptions__content) {
  color: #0f172a;
  background: rgba(255, 255, 255, 0.98);
}

.dashboard-page__summary {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 10px;
}

.dashboard-page__summary-card {
  padding: 14px 16px;
  transition:
    transform 0.2s ease,
    box-shadow 0.2s ease,
    border-color 0.2s ease;
}

.dashboard-page__summary-card:hover {
  transform: translateY(-1px);
  border-color: rgba(59, 130, 246, 0.24);
  box-shadow:
    0 16px 28px rgba(15, 23, 42, 0.06),
    0 6px 14px rgba(59, 130, 246, 0.08);
}

.dashboard-page__summary-label {
  margin: 0 0 12px;
  color: #64748b;
  font-size: 12px;
  font-weight: 600;
  line-height: 1.4;
}

.dashboard-page__summary-value {
  color: #0f172a;
  font-size: clamp(28px, 2.6vw, 34px);
  line-height: 1;
  letter-spacing: -0.04em;
}

.dashboard-page__detail {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
  min-width: 0;
}

.dashboard-page__detail-card :deep(.el-card__header) {
  padding: 10px 14px 0;
  border-bottom: none;
}

.dashboard-page__detail-card :deep(.el-card__body) {
  padding: 8px 14px 14px;
}

.dashboard-page__tag-list,
.dashboard-page__button-row {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.dashboard-page__tag-list :deep(.el-tag) {
  min-height: 28px;
  padding: 0 10px;
  border-radius: 999px;
  font-weight: 600;
}

.dashboard-page__button-row :deep(.el-button) {
  min-height: 38px;
  border-radius: 10px;
  font-weight: 600;
}

.dashboard-page__empty {
  color: #64748b;
  font-size: 13px;
  line-height: 1.5;
}

@media (max-width: 1080px) {
  .dashboard-page__hero,
  .dashboard-page__detail {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 920px) {
  .dashboard-page {
    gap: 12px;
  }

  .dashboard-page__summary {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .dashboard-page {
    gap: 10px;
  }

  .dashboard-page__hero-main,
  .dashboard-page__profile-card {
    padding: 16px;
  }

  .dashboard-page__title {
    font-size: clamp(22px, 7vw, 26px);
  }

  .dashboard-page__summary {
    grid-template-columns: 1fr;
  }

  .dashboard-page__detail-card :deep(.el-card__header) {
    padding: 10px 12px 0;
  }

  .dashboard-page__detail-card :deep(.el-card__body) {
    padding: 8px 12px 12px;
  }
}
</style>
