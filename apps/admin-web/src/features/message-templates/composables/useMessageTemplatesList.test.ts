import { beforeEach, describe, expect, it, vi } from 'vitest'

import { useMessageTemplatesList } from '@/features/message-templates/composables/useMessageTemplatesList'

const messageTemplatesApi = vi.hoisted(() => ({
  getPagedMessageTemplates: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
}))

vi.mock('@/features/message-templates/api/messageTemplatesApi', () => messageTemplatesApi)
vi.mock('element-plus', () => ({
  ElMessage: message,
}))

describe('useMessageTemplatesList', () => {
  beforeEach(() => {
    messageTemplatesApi.getPagedMessageTemplates.mockReset()
    message.error.mockReset()
  })

  it('loads templates and updates result state', async () => {
    messageTemplatesApi.getPagedMessageTemplates.mockResolvedValue({
      totalCount: 1,
      items: [
        {
          id: 'template-1',
          code: 'order.pay_success.sms',
          name: 'Paid Notice',
          description: 'Notify user after order payment succeeds',
          isEnabled: true,
          channelType: 1,
        },
      ],
    })

    const list = useMessageTemplatesList()
    await list.loadTemplates()

    expect(messageTemplatesApi.getPagedMessageTemplates).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 10,
    })
    expect(list.templates.value).toHaveLength(1)
    expect(list.totalCount.value).toBe(1)
    expect(list.isLoading.value).toBe(false)
  })

  it('resets page index and reloads with current filters on search', async () => {
    messageTemplatesApi.getPagedMessageTemplates.mockResolvedValue({
      totalCount: 0,
      items: [],
    })

    const list = useMessageTemplatesList()
    list.pagination.pageIndex = 3
    list.filters.keyword = '  order  '
    list.filters.status = 'disabled'

    await list.handleSearch()

    expect(list.pagination.pageIndex).toBe(1)
    expect(messageTemplatesApi.getPagedMessageTemplates).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 10,
      keyword: 'order',
      isEnabled: false,
    })
  })

  it('clears stale data and shows an error message when loading fails', async () => {
    messageTemplatesApi.getPagedMessageTemplates.mockRejectedValue(new Error('request failed'))

    const list = useMessageTemplatesList()
    list.templates.value = [
      {
        id: 'template-1',
        code: 'stale.site_message',
        name: 'Stale Template',
        description: null,
        isEnabled: true,
        channelType: 3,
      },
    ]
    list.totalCount.value = 1

    await list.loadTemplates()

    expect(list.templates.value).toEqual([])
    expect(list.totalCount.value).toBe(0)
    expect(message.error).toHaveBeenCalledWith('request failed')
  })
})
