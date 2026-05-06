<script setup lang="ts">
import { View } from '@element-plus/icons-vue'

import {
  formatSiteMessageStatus,
  formatSiteMessageTitle,
  resolveSiteMessageStatusType,
} from '@/features/site-messages/models/siteMessagesList'
import type {
  SiteMessageBasicResponse,
  SiteMessagesPagination,
} from '@/features/site-messages/types/siteMessages'
import { formatDateTime } from '@/shared/utils/dateTime'

defineProps<{
  isLoading: boolean
  items: SiteMessageBasicResponse[]
  pagination: SiteMessagesPagination
  totalCount: number
}>()

const emit = defineEmits<{
  detail: [message: SiteMessageBasicResponse]
  'page-change': [pageIndex: number]
  'page-size-change': [pageSize: number]
}>()

function handleRowClick(row: SiteMessageBasicResponse) {
  emit('detail', row)
}
</script>

<template>
  <div class="site-messages-table-card">
    <el-table
      v-loading="isLoading"
      :data="items"
      class="site-messages-table-card__table"
      empty-text="暂无站内信"
      height="100%"
      @row-click="handleRowClick"
    >
      <el-table-column label="消息标题" min-width="240">
        <template #default="{ row }">
          <div class="site-messages-table-card__title-cell">
            <strong>{{ formatSiteMessageTitle(row) }}</strong>
            <span>{{ row.contentPreview }}</span>
          </div>
        </template>
      </el-table-column>

      <el-table-column label="状态" width="110">
        <template #default="{ row }">
          <el-tag size="small" :type="resolveSiteMessageStatusType(row.isRead)">
            {{ formatSiteMessageStatus(row.isRead) }}
          </el-tag>
        </template>
      </el-table-column>

      <el-table-column label="创建时间" min-width="160">
        <template #default="{ row }">
          {{ formatDateTime(row.createdAt) }}
        </template>
      </el-table-column>

      <el-table-column label="已读时间" min-width="160">
        <template #default="{ row }">
          {{ formatDateTime(row.readAt) }}
        </template>
      </el-table-column>

      <el-table-column label="操作" width="120" fixed="right">
        <template #default="{ row }">
          <el-button :icon="View" link type="primary" @click.stop="$emit('detail', row)">
            查看详情
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <div class="site-messages-table-card__pagination">
      <el-pagination
        :current-page="pagination.pageIndex"
        :page-size="pagination.pageSize"
        :page-sizes="[10, 20, 50]"
        :total="totalCount"
        background
        layout="total, sizes, prev, pager, next"
        @current-change="$emit('page-change', $event)"
        @size-change="$emit('page-size-change', $event)"
      />
    </div>
  </div>
</template>

<style scoped>
.site-messages-table-card {
  display: grid;
  grid-template-rows: minmax(0, 1fr) auto;
  min-height: 0;
  height: 100%;
  padding: 10px 10px 14px;
  border: 1px solid rgba(226, 232, 240, 0.9);
  border-radius: 18px;
  background: linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.92));
}

.site-messages-table-card__table {
  min-height: 0;
}

.site-messages-table-card__table :deep(.el-table__inner-wrapper::before) {
  display: none;
}

.site-messages-table-card__table :deep(.el-table__row) {
  cursor: pointer;
}

.site-messages-table-card__title-cell {
  display: grid;
  gap: 6px;
}

.site-messages-table-card__title-cell strong {
  color: #0f172a;
  line-height: 1.4;
}

.site-messages-table-card__title-cell span {
  color: #64748b;
  font-size: 13px;
  line-height: 1.5;
}

.site-messages-table-card__pagination {
  display: flex;
  justify-content: flex-end;
  padding-top: 14px;
}

@media (max-width: 920px) {
  .site-messages-table-card {
    height: auto;
  }

  .site-messages-table-card__table {
    height: auto;
  }

  .site-messages-table-card__table :deep(.el-table) {
    height: auto !important;
  }

  .site-messages-table-card__pagination {
    justify-content: flex-start;
    overflow-x: auto;
  }
}
</style>
