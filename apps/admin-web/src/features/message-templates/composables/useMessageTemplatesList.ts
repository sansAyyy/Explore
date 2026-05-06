import { ElMessage } from 'element-plus'
import { computed, reactive, ref } from 'vue'

import { getPagedMessageTemplates } from '@/features/message-templates/api/messageTemplatesApi'
import {
  buildGetPagedMessageTemplatesQuery,
  createDefaultMessageTemplatesFilters,
  createDefaultMessageTemplatesPagination,
  messageTemplatesStatusOptions,
} from '@/features/message-templates/models/messageTemplatesList'
import type { MessageTemplateBasicResponse } from '@/features/message-templates/types/messageTemplates'
import { extractErrorMessage } from '@/shared/api/error'

export function useMessageTemplatesList() {
  const templates = ref<MessageTemplateBasicResponse[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const filters = reactive(createDefaultMessageTemplatesFilters())
  const pagination = reactive(createDefaultMessageTemplatesPagination())
  const querySummary = computed(() => buildGetPagedMessageTemplatesQuery(filters, pagination))

  async function loadTemplates() {
    isLoading.value = true

    try {
      const result = await getPagedMessageTemplates(querySummary.value)
      templates.value = result.items
      totalCount.value = result.totalCount
    } catch (error) {
      templates.value = []
      totalCount.value = 0
      ElMessage.error(extractErrorMessage(error, '加载消息模板失败'))
    } finally {
      isLoading.value = false
    }
  }

  async function handleSearch() {
    pagination.pageIndex = 1
    await loadTemplates()
  }

  async function handleReset() {
    Object.assign(filters, createDefaultMessageTemplatesFilters())
    Object.assign(pagination, createDefaultMessageTemplatesPagination())
    await loadTemplates()
  }

  async function handlePageChange(pageIndex: number) {
    pagination.pageIndex = pageIndex
    await loadTemplates()
  }

  async function handlePageSizeChange(pageSize: number) {
    pagination.pageSize = pageSize
    pagination.pageIndex = 1
    await loadTemplates()
  }

  return {
    filters,
    handlePageChange,
    handlePageSizeChange,
    handleReset,
    handleSearch,
    isLoading,
    loadTemplates,
    pagination,
    querySummary,
    statusOptions: messageTemplatesStatusOptions,
    templates,
    totalCount,
  }
}
