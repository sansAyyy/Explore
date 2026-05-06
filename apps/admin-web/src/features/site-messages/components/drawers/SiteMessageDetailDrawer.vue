<script setup lang="ts">
import {
  formatSiteMessageTitle,
  formatSiteMessageStatus,
  resolveSiteMessageStatusType,
} from '@/features/site-messages/models/siteMessagesList'
import type {
  SiteMessageBasicResponse,
  SiteMessageDetailResponse,
} from '@/features/site-messages/types/siteMessages'
import { formatDateTime } from '@/shared/utils/dateTime'

defineProps<{
  detail: SiteMessageDetailResponse | null
  isLoading: boolean
  message: SiteMessageBasicResponse | null
  modelValue: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

function closeDrawer() {
  emit('update:modelValue', false)
}
</script>

<template>
  <el-drawer
    :model-value="modelValue"
    title="站内信详情"
    size="680px"
    destroy-on-close
    class="site-message-detail-drawer"
    @close="closeDrawer"
  >
    <div v-loading="isLoading" class="site-message-detail-drawer__panel">
      <template v-if="detail">
        <div class="site-message-detail-drawer__hero">
          <div class="site-message-detail-drawer__summary">
            <div class="site-message-detail-drawer__title-row">
              <strong>{{ formatSiteMessageTitle(detail) }}</strong>
              <el-tag size="small" :type="resolveSiteMessageStatusType(detail.isRead)">
                {{ formatSiteMessageStatus(detail.isRead) }}
              </el-tag>
            </div>

            <div class="site-message-detail-drawer__meta">
              <span>创建时间：{{ formatDateTime(detail.createdAt) }}</span>
              <span>已读时间：{{ formatDateTime(detail.readAt) }}</span>
            </div>
          </div>
        </div>

        <el-descriptions :column="1" border class="site-message-detail-drawer__descriptions">
          <el-descriptions-item label="消息标题">
            {{ formatSiteMessageTitle(detail) }}
          </el-descriptions-item>
          <el-descriptions-item label="消息正文">
            <div class="site-message-detail-drawer__content">
              {{ detail.content }}
            </div>
          </el-descriptions-item>
          <el-descriptions-item label="消息编号">
            {{ detail.id }}
          </el-descriptions-item>
          <el-descriptions-item label="投递编号">
            {{ detail.dispatchId }}
          </el-descriptions-item>
        </el-descriptions>
      </template>

      <div v-else class="site-message-detail-drawer__empty">
        <el-empty :description="message ? '未加载到站内信详情' : '请选择站内信'" />
      </div>
    </div>
  </el-drawer>
</template>

<style scoped>
.site-message-detail-drawer :deep(.el-drawer) {
  background:
    radial-gradient(circle at top right, rgba(37, 99, 235, 0.08), transparent 32%),
    linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.site-message-detail-drawer :deep(.el-drawer__header) {
  margin-bottom: 0;
  padding: 22px 24px 18px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.92);
}

.site-message-detail-drawer :deep(.el-drawer__title) {
  color: #0f172a;
  font-size: 18px;
  font-weight: 700;
  letter-spacing: -0.02em;
}

.site-message-detail-drawer :deep(.el-drawer__body) {
  padding: 22px 24px 18px;
}

.site-message-detail-drawer__panel {
  display: grid;
  gap: 18px;
  min-height: 0;
}

.site-message-detail-drawer__hero {
  padding: 18px 20px;
  border: 1px solid rgba(148, 163, 184, 0.2);
  border-radius: 18px;
  background: linear-gradient(135deg, rgba(30, 64, 175, 0.08), rgba(59, 130, 246, 0.03));
}

.site-message-detail-drawer__summary {
  display: grid;
  gap: 10px;
}

.site-message-detail-drawer__title-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.site-message-detail-drawer__title-row strong {
  color: #0f172a;
  font-size: 20px;
  line-height: 1.2;
}

.site-message-detail-drawer__meta {
  display: flex;
  flex-wrap: wrap;
  gap: 10px 18px;
  color: #64748b;
  font-size: 13px;
}

.site-message-detail-drawer__descriptions :deep(.el-descriptions__body) {
  border-radius: 16px;
  overflow: hidden;
}

.site-message-detail-drawer__descriptions :deep(.el-descriptions__label) {
  width: 120px;
  color: #334155;
  font-weight: 600;
  background: rgba(248, 250, 252, 0.9);
}

.site-message-detail-drawer__descriptions :deep(.el-descriptions__content) {
  color: #0f172a;
  background: rgba(255, 255, 255, 0.96);
}

.site-message-detail-drawer__content {
  white-space: pre-wrap;
  word-break: break-word;
  line-height: 1.7;
}

.site-message-detail-drawer__empty {
  display: grid;
  place-items: center;
  min-height: 240px;
  padding: 12px;
  border: 1px dashed rgba(148, 163, 184, 0.28);
  border-radius: 14px;
  background: linear-gradient(180deg, rgba(248, 250, 252, 0.72), rgba(255, 255, 255, 0.94));
}

.site-message-detail-drawer__empty :deep(.el-empty__description p) {
  color: #64748b;
}

@media (max-width: 640px) {
  .site-message-detail-drawer :deep(.el-drawer__header) {
    padding: 18px 16px 14px;
  }

  .site-message-detail-drawer :deep(.el-drawer__body) {
    padding: 18px 16px 14px;
  }

  .site-message-detail-drawer__title-row {
    flex-direction: column;
  }
}
</style>
