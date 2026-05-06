<script setup lang="ts">
import { Check, RefreshRight } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import SiteMessageDetailDrawer from '@/features/site-messages/components/drawers/SiteMessageDetailDrawer.vue'
import SiteMessagesTable from '@/features/site-messages/components/list/SiteMessagesTable.vue'
import { useSiteMessageDetailDrawer } from '@/features/site-messages/composables/useSiteMessageDetailDrawer'
import { useSiteMessagesList } from '@/features/site-messages/composables/useSiteMessagesList'
import type { SiteMessageBasicResponse } from '@/features/site-messages/types/siteMessages'
import { useSiteMessagesStore } from '@/stores/siteMessages'

const route = useRoute()
const router = useRouter()
const siteMessagesStore = useSiteMessagesStore()
const list = useSiteMessagesList()

const detailDrawer = useSiteMessageDetailDrawer({
  async onMessageRead(id, readAt) {
    list.markMessageReadLocal(id, readAt)
    await siteMessagesStore.refreshPreviewContext({ silent: true })
  },
})

const hasUnreadMessages = computed(() => siteMessagesStore.unreadCount > 0)
const unreadSummaryText = computed(() => `当前未读 ${siteMessagesStore.unreadCount} 条`)

async function handleMarkAllRead() {
  try {
    await siteMessagesStore.markAllRead()
    ElMessage.success('已全部标记为已读')
  } catch {
    // Error feedback is handled in the shared store.
  }
}

async function handleRefresh() {
  await Promise.all([
    list.handleRefresh(),
    siteMessagesStore.refreshUnreadCount({ silent: true }),
  ])
}

function resolveMessageById(messageId: string): SiteMessageBasicResponse | null {
  return (
    list.messages.value.find((message) => message.id === messageId)
    ?? siteMessagesStore.previewItems.find((message) => message.id === messageId)
    ?? null
  )
}

async function consumeMessageIdQuery() {
  const messageId = typeof route.query.messageId === 'string' ? route.query.messageId : ''
  if (!messageId) {
    return
  }

  await detailDrawer.handleOpenDetailDrawerById(messageId, resolveMessageById(messageId))
  await router.replace({
    name: 'site-messages',
  })
}

onMounted(async () => {
  await list.loadMessages()
  await consumeMessageIdQuery()
})

watch(
  () => siteMessagesStore.lastMarkedAllReadAt,
  (markedAt) => {
    if (!markedAt) {
      return
    }

    list.markAllMessagesReadLocal(markedAt)
  },
)

watch(
  () => route.query.messageId,
  (messageId, previousMessageId) => {
    if (messageId === previousMessageId || typeof messageId !== 'string' || !messageId) {
      return
    }

    void detailDrawer.handleOpenDetailDrawerById(messageId, resolveMessageById(messageId))
    void router.replace({
      name: 'site-messages',
    })
  },
)
</script>

<template>
  <div class="site-messages-page">
    <section class="site-messages-page__hero">
      <div>
        <p class="site-messages-page__eyebrow">Inbox Center</p>
        <h1 class="site-messages-page__title">站内信</h1>
      </div>

      <div class="site-messages-page__hero-actions">
        <el-tag :type="hasUnreadMessages ? 'danger' : 'info'" effect="light" round>
          {{ unreadSummaryText }}
        </el-tag>

        <el-button :icon="RefreshRight" text type="primary" @click="handleRefresh">
          刷新列表
        </el-button>
      </div>
    </section>

    <section class="site-messages-page__toolbar">
      <div class="site-messages-page__filter-group">
        <span class="site-messages-page__filter-label">筛选范围</span>

        <el-radio-group
          :model-value="list.filters.readStatus"
          size="large"
          @update:model-value="list.handleReadStatusChange"
        >
          <el-radio-button
            v-for="option in list.readStatusOptions"
            :key="option.value"
            :label="option.value"
          >
            {{ option.label }}
          </el-radio-button>
        </el-radio-group>
      </div>

      <div class="site-messages-page__toolbar-actions">
        <el-button @click="list.handleReset">重置筛选</el-button>
        <el-button
          :disabled="siteMessagesStore.unreadCount === 0"
          :icon="Check"
          :loading="siteMessagesStore.isMarkingAllRead"
          type="primary"
          @click="handleMarkAllRead"
        >
          全部标记已读
        </el-button>
      </div>
    </section>

    <section class="site-messages-page__table">
      <SiteMessagesTable
        :is-loading="list.isLoading.value"
        :items="list.messages.value"
        :pagination="list.pagination"
        :total-count="list.totalCount.value"
        @detail="detailDrawer.handleOpenDetailDrawer"
        @page-change="list.handlePageChange"
        @page-size-change="list.handlePageSizeChange"
      />
    </section>

    <SiteMessageDetailDrawer
      :detail="detailDrawer.detail.value"
      :is-loading="detailDrawer.isLoading.value"
      :message="detailDrawer.selectedMessage.value"
      :model-value="detailDrawer.isDetailDrawerOpen.value"
      @update:model-value="
        (value) => {
          if (!value) {
            detailDrawer.handleCloseDetailDrawer()
          }
        }
      "
    />
  </div>
</template>

<style scoped>
.site-messages-page {
  display: grid;
  grid-template-rows: auto auto minmax(0, 1fr);
  gap: 12px;
  min-height: 0;
  height: 100%;
  color: #334155;
  overflow: hidden;
}

.site-messages-page__hero {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  padding: 0 2px 8px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.9);
}

.site-messages-page__eyebrow {
  margin: 0 0 4px;
  color: #64748b;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.site-messages-page__title {
  margin: 0;
  color: #0f172a;
  font-size: clamp(22px, 2vw, 26px);
  font-weight: 750;
  line-height: 1.04;
  letter-spacing: -0.03em;
}

.site-messages-page__hero-actions {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 8px 12px;
}

.site-messages-page__toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 18px 20px;
  border: 1px solid rgba(226, 232, 240, 0.92);
  border-radius: 20px;
  background:
    radial-gradient(circle at top left, rgba(59, 130, 246, 0.08), transparent 32%),
    linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.94));
}

.site-messages-page__filter-group {
  display: grid;
  gap: 10px;
}

.site-messages-page__filter-label {
  color: #64748b;
  font-size: 12px;
  font-weight: 600;
}

.site-messages-page__toolbar-actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 10px;
}

.site-messages-page__table {
  min-height: 0;
}

@media (max-width: 920px) {
  .site-messages-page {
    grid-template-rows: auto auto auto;
    height: auto;
    overflow: visible;
  }

  .site-messages-page__hero,
  .site-messages-page__toolbar {
    flex-direction: column;
    align-items: flex-start;
  }

  .site-messages-page__hero-actions,
  .site-messages-page__toolbar-actions {
    justify-content: flex-start;
  }
}
</style>
