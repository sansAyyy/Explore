import { ElMessage } from 'element-plus'
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

import {
  getPagedSiteMessages,
  markAllSiteMessagesRead,
} from '@/features/site-messages/api/siteMessagesApi'
import { extractErrorMessage } from '@/shared/api/error'

interface SiteMessageRequestOptions {
  silent?: boolean
}

export const useSiteMessagesStore = defineStore('site-messages', () => {
  const unreadCount = ref(0)
  const previewItems = ref<Awaited<ReturnType<typeof getPagedSiteMessages>>['items']>([])
  const isPreviewDrawerOpen = ref(false)
  const isPreviewLoading = ref(false)
  const isUnreadCountLoading = ref(false)
  const isMarkingAllRead = ref(false)
  const lastMarkedAllReadAt = ref<string | null>(null)

  const hasUnread = computed(() => unreadCount.value > 0)
  const unreadCountBadgeValue = computed(() =>
    unreadCount.value > 99 ? '99+' : unreadCount.value.toString(),
  )

  function notifyError(error: unknown, fallbackMessage: string, silent: boolean) {
    if (!silent) {
      ElMessage.error(extractErrorMessage(error, fallbackMessage))
    }
  }

  async function refreshUnreadCount(options: SiteMessageRequestOptions = {}) {
    const { silent = false } = options
    isUnreadCountLoading.value = true

    try {
      const result = await getPagedSiteMessages({
        pageIndex: 1,
        pageSize: 1,
        isRead: false,
      })

      unreadCount.value = result.totalCount
      return result.totalCount
    } catch (error) {
      unreadCount.value = 0
      notifyError(error, '加载站内信未读数失败', silent)
      return 0
    } finally {
      isUnreadCountLoading.value = false
    }
  }

  async function loadPreviewItems(options: SiteMessageRequestOptions = {}) {
    const { silent = false } = options
    isPreviewLoading.value = true

    try {
      const result = await getPagedSiteMessages({
        pageIndex: 1,
        pageSize: 5,
      })

      previewItems.value = result.items
      return result.items
    } catch (error) {
      previewItems.value = []
      notifyError(error, '加载站内信预览失败', silent)
      return []
    } finally {
      isPreviewLoading.value = false
    }
  }

  async function refreshPreviewContext(options: SiteMessageRequestOptions = {}) {
    await Promise.all([refreshUnreadCount(options), loadPreviewItems(options)])
  }

  async function openPreviewDrawer() {
    isPreviewDrawerOpen.value = true
    await refreshPreviewContext()
  }

  function closePreviewDrawer() {
    isPreviewDrawerOpen.value = false
  }

  async function markAllRead() {
    isMarkingAllRead.value = true

    try {
      await markAllSiteMessagesRead()
      lastMarkedAllReadAt.value = new Date().toISOString()
      await refreshPreviewContext({ silent: true })
    } catch (error) {
      ElMessage.error(extractErrorMessage(error, '全部标记已读失败'))
      throw error
    } finally {
      isMarkingAllRead.value = false
    }
  }

  function clear() {
    unreadCount.value = 0
    previewItems.value = []
    isPreviewDrawerOpen.value = false
    isPreviewLoading.value = false
    isUnreadCountLoading.value = false
    isMarkingAllRead.value = false
    lastMarkedAllReadAt.value = null
  }

  return {
    clear,
    closePreviewDrawer,
    hasUnread,
    isMarkingAllRead,
    isPreviewDrawerOpen,
    isPreviewLoading,
    isUnreadCountLoading,
    lastMarkedAllReadAt,
    loadPreviewItems,
    markAllRead,
    openPreviewDrawer,
    previewItems,
    refreshPreviewContext,
    refreshUnreadCount,
    unreadCount,
    unreadCountBadgeValue,
  }
})
