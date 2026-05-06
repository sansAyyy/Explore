import { ElMessage } from 'element-plus'
import { ref } from 'vue'

import { getSiteMessageById, markSiteMessageRead } from '@/features/site-messages/api/siteMessagesApi'
import type {
  SiteMessageBasicResponse,
  SiteMessageDetailResponse,
} from '@/features/site-messages/types/siteMessages'
import { extractErrorMessage } from '@/shared/api/error'

interface UseSiteMessageDetailDrawerOptions {
  onMessageRead?: (id: string, readAt: string) => Promise<void> | void
}

export function useSiteMessageDetailDrawer(options: UseSiteMessageDetailDrawerOptions = {}) {
  const isDetailDrawerOpen = ref(false)
  const selectedMessage = ref<SiteMessageBasicResponse | null>(null)
  const detail = ref<SiteMessageDetailResponse | null>(null)
  const isLoading = ref(false)
  let requestSequence = 0

  async function loadDetail(messageId: string, message: SiteMessageBasicResponse | null) {
    const currentRequest = ++requestSequence
    isLoading.value = true
    detail.value = null

    try {
      const shouldMarkRead = message ? !message.isRead : true

      if (shouldMarkRead) {
        await markSiteMessageRead(messageId)
      }

      const result = await getSiteMessageById(messageId)
      if (currentRequest !== requestSequence) {
        return
      }

      detail.value = result
      selectedMessage.value = message

      if (shouldMarkRead) {
        await options.onMessageRead?.(messageId, result.readAt ?? new Date().toISOString())
      }
    } catch (error) {
      if (currentRequest === requestSequence) {
        ElMessage.error(extractErrorMessage(error, '加载站内信详情失败'))
        handleCloseDetailDrawer()
      }
    } finally {
      if (currentRequest === requestSequence) {
        isLoading.value = false
      }
    }
  }

  async function handleOpenDetailDrawer(message: SiteMessageBasicResponse) {
    selectedMessage.value = message
    isDetailDrawerOpen.value = true
    await loadDetail(message.id, message)
  }

  async function handleOpenDetailDrawerById(
    messageId: string,
    message: SiteMessageBasicResponse | null = null,
  ) {
    selectedMessage.value = message
    isDetailDrawerOpen.value = true
    await loadDetail(messageId, message)
  }

  function handleCloseDetailDrawer() {
    requestSequence += 1
    isDetailDrawerOpen.value = false
    selectedMessage.value = null
    detail.value = null
    isLoading.value = false
  }

  return {
    detail,
    handleCloseDetailDrawer,
    handleOpenDetailDrawer,
    handleOpenDetailDrawerById,
    isDetailDrawerOpen,
    isLoading,
    selectedMessage,
  }
}
