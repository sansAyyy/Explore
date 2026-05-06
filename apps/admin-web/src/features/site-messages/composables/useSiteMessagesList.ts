import { ElMessage } from 'element-plus'
import { computed, reactive, ref } from 'vue'

import { getPagedSiteMessages } from '@/features/site-messages/api/siteMessagesApi'
import {
  buildGetPagedSiteMessagesQuery,
  createDefaultSiteMessagesFilters,
  createDefaultSiteMessagesPagination,
  siteMessagesReadStatusOptions,
} from '@/features/site-messages/models/siteMessagesList'
import type { SiteMessageBasicResponse } from '@/features/site-messages/types/siteMessages'
import { extractErrorMessage } from '@/shared/api/error'

export function useSiteMessagesList() {
  const messages = ref<SiteMessageBasicResponse[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const filters = reactive(createDefaultSiteMessagesFilters())
  const pagination = reactive(createDefaultSiteMessagesPagination())
  const querySummary = computed(() => buildGetPagedSiteMessagesQuery(filters, pagination))

  async function loadMessages() {
    isLoading.value = true

    try {
      const result = await getPagedSiteMessages(querySummary.value)
      messages.value = result.items
      totalCount.value = result.totalCount
    } catch (error) {
      messages.value = []
      totalCount.value = 0
      ElMessage.error(extractErrorMessage(error, '加载站内信列表失败'))
    } finally {
      isLoading.value = false
    }
  }

  async function handleReadStatusChange(readStatus: typeof filters.readStatus) {
    filters.readStatus = readStatus
    pagination.pageIndex = 1
    await loadMessages()
  }

  async function handleRefresh() {
    await loadMessages()
  }

  async function handleReset() {
    Object.assign(filters, createDefaultSiteMessagesFilters())
    Object.assign(pagination, createDefaultSiteMessagesPagination())
    await loadMessages()
  }

  async function handlePageChange(pageIndex: number) {
    pagination.pageIndex = pageIndex
    await loadMessages()
  }

  async function handlePageSizeChange(pageSize: number) {
    pagination.pageSize = pageSize
    pagination.pageIndex = 1
    await loadMessages()
  }

  function markMessageReadLocal(id: string, readAt: string) {
    const nextItems = [...messages.value]
    const targetIndex = nextItems.findIndex((message) => message.id === id)

    if (targetIndex < 0) {
      return
    }

    const target = nextItems[targetIndex]
    if (target.isRead) {
      return
    }

    if (filters.readStatus === 'unread') {
      nextItems.splice(targetIndex, 1)
      messages.value = nextItems
      totalCount.value = Math.max(0, totalCount.value - 1)
      return
    }

    nextItems[targetIndex] = {
      ...target,
      isRead: true,
      readAt,
    }

    messages.value = nextItems
  }

  function markAllMessagesReadLocal(readAt: string) {
    if (filters.readStatus === 'unread') {
      messages.value = []
      totalCount.value = 0
      return
    }

    messages.value = messages.value.map((message) =>
      message.isRead
        ? message
        : {
            ...message,
            isRead: true,
            readAt,
          },
    )
  }

  return {
    filters,
    handlePageChange,
    handlePageSizeChange,
    handleReadStatusChange,
    handleRefresh,
    handleReset,
    isLoading,
    loadMessages,
    markAllMessagesReadLocal,
    markMessageReadLocal,
    messages,
    pagination,
    querySummary,
    readStatusOptions: siteMessagesReadStatusOptions,
    totalCount,
  }
}
