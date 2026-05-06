<script setup lang="ts">
import { ArrowRight, Bell, Check, MoreFilled } from '@element-plus/icons-vue'

import {
  formatSiteMessageTitle,
  formatSiteMessageStatus,
  resolveSiteMessageStatusType,
} from '@/features/site-messages/models/siteMessagesList'
import type { SiteMessageBasicResponse } from '@/features/site-messages/types/siteMessages'
import { formatDateTime } from '@/shared/utils/dateTime'

defineProps<{
  isLoading: boolean
  isMarkingAllRead: boolean
  items: SiteMessageBasicResponse[]
  modelValue: boolean
  unreadCount: number
}>()

const emit = defineEmits<{
  'mark-all-read': []
  'open-message': [message: SiteMessageBasicResponse]
  'update:modelValue': [value: boolean]
  'view-all': []
}>()

function closeDrawer() {
  emit('update:modelValue', false)
}
</script>

<template>
  <el-drawer
    :model-value="modelValue"
    :with-header="false"
    append-to-body
    class="site-messages-preview-drawer"
    direction="rtl"
    size="420px"
    @close="closeDrawer"
  >
    <div class="site-messages-preview-drawer__shell">
      <header class="site-messages-preview-drawer__header">
        <div class="site-messages-preview-drawer__title-group">
          <span class="site-messages-preview-drawer__icon">
            <el-icon><Bell /></el-icon>
          </span>
          <div>
            <p>站内信预览</p>
            <strong>{{ unreadCount }} 条未读</strong>
          </div>
        </div>

        <div class="site-messages-preview-drawer__actions">
          <el-button
            :disabled="unreadCount === 0"
            :icon="Check"
            :loading="isMarkingAllRead"
            text
            type="primary"
            @click="$emit('mark-all-read')"
          >
            全部标记已读
          </el-button>

          <el-button :icon="ArrowRight" text type="primary" @click="$emit('view-all')">
            进入收件箱
          </el-button>
        </div>
      </header>

      <el-scrollbar v-loading="isLoading" class="site-messages-preview-drawer__list">
        <template v-if="items.length > 0">
          <button
            v-for="item in items"
            :key="item.id"
            type="button"
            class="site-messages-preview-drawer__item"
            @click="$emit('open-message', item)"
          >
            <div class="site-messages-preview-drawer__item-head">
              <strong>{{ formatSiteMessageTitle(item) }}</strong>
              <el-tag size="small" :type="resolveSiteMessageStatusType(item.isRead)">
                {{ formatSiteMessageStatus(item.isRead) }}
              </el-tag>
            </div>

            <p>{{ item.contentPreview }}</p>

            <div class="site-messages-preview-drawer__item-foot">
              <span>{{ formatDateTime(item.createdAt) }}</span>
              <el-icon><MoreFilled /></el-icon>
            </div>
          </button>
        </template>

        <div v-else class="site-messages-preview-drawer__empty">
          <el-empty description="暂无站内信" />
        </div>
      </el-scrollbar>
    </div>
  </el-drawer>
</template>

<style scoped>
.site-messages-preview-drawer :deep(.el-drawer) {
  background:
    radial-gradient(circle at top right, rgba(59, 130, 246, 0.12), transparent 30%),
    linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.site-messages-preview-drawer :deep(.el-drawer__body) {
  padding: 0;
}

.site-messages-preview-drawer__shell {
  display: grid;
  grid-template-rows: auto minmax(0, 1fr);
  height: 100%;
}

.site-messages-preview-drawer__header {
  display: grid;
  gap: 14px;
  padding: 22px 22px 18px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.92);
}

.site-messages-preview-drawer__title-group {
  display: flex;
  align-items: center;
  gap: 14px;
}

.site-messages-preview-drawer__icon {
  display: inline-grid;
  place-items: center;
  width: 44px;
  height: 44px;
  border-radius: 14px;
  background: linear-gradient(135deg, #1d4ed8 0%, #3b82f6 100%);
  color: #ffffff;
  box-shadow: 0 12px 24px rgba(37, 99, 235, 0.24);
}

.site-messages-preview-drawer__title-group p {
  margin: 0 0 4px;
  color: #64748b;
  font-size: 12px;
}

.site-messages-preview-drawer__title-group strong {
  color: #0f172a;
  font-size: 20px;
  letter-spacing: -0.02em;
}

.site-messages-preview-drawer__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 6px 12px;
}

.site-messages-preview-drawer__list {
  min-height: 0;
  padding: 18px 14px 18px 18px;
}

.site-messages-preview-drawer__item {
  display: grid;
  gap: 10px;
  width: 100%;
  padding: 16px;
  margin-bottom: 12px;
  border: 1px solid rgba(203, 213, 225, 0.72);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.94);
  text-align: left;
  cursor: pointer;
  transition:
    transform 0.18s ease,
    border-color 0.18s ease,
    box-shadow 0.18s ease;
}

.site-messages-preview-drawer__item:hover {
  transform: translateY(-1px);
  border-color: rgba(96, 165, 250, 0.48);
  box-shadow: 0 16px 28px rgba(15, 23, 42, 0.08);
}

.site-messages-preview-drawer__item-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.site-messages-preview-drawer__item-head strong {
  color: #0f172a;
  line-height: 1.4;
}

.site-messages-preview-drawer__item p {
  margin: 0;
  color: #475569;
  font-size: 13px;
  line-height: 1.6;
}

.site-messages-preview-drawer__item-foot {
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: #64748b;
  font-size: 12px;
}

.site-messages-preview-drawer__empty {
  display: grid;
  place-items: center;
  min-height: 260px;
}

@media (max-width: 640px) {
  .site-messages-preview-drawer :deep(.el-drawer) {
    width: min(100vw, 420px) !important;
  }

  .site-messages-preview-drawer__header {
    padding: 18px 16px 14px;
  }

  .site-messages-preview-drawer__list {
    padding: 14px 12px 16px;
  }
}
</style>
