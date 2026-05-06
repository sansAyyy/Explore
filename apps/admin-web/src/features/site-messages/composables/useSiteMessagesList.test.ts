import { beforeEach, describe, expect, it, vi } from 'vitest'

import { useSiteMessagesList } from '@/features/site-messages/composables/useSiteMessagesList'

const siteMessagesApi = vi.hoisted(() => ({
  getPagedSiteMessages: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
}))

vi.mock('@/features/site-messages/api/siteMessagesApi', () => siteMessagesApi)
vi.mock('element-plus', () => ({
  ElMessage: message,
}))

describe('useSiteMessagesList', () => {
  beforeEach(() => {
    siteMessagesApi.getPagedSiteMessages.mockReset()
    message.error.mockReset()
  })

  it('loads site messages with the current query state', async () => {
    siteMessagesApi.getPagedSiteMessages.mockResolvedValue({
      totalCount: 1,
      items: [
        {
          id: 'message-1',
          userId: 'user-1',
          title: '系统通知',
          contentPreview: 'Preview',
          isRead: false,
          createdAt: '2026-05-01T00:00:00Z',
          readAt: null,
        },
      ],
    })

    const list = useSiteMessagesList()
    await list.loadMessages()

    expect(siteMessagesApi.getPagedSiteMessages).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 10,
    })
    expect(list.messages.value).toHaveLength(1)
    expect(list.totalCount.value).toBe(1)
  })

  it('maps the unread filter when changing the read status', async () => {
    siteMessagesApi.getPagedSiteMessages.mockResolvedValue({
      totalCount: 0,
      items: [],
    })

    const list = useSiteMessagesList()
    list.pagination.pageIndex = 3

    await list.handleReadStatusChange('unread')

    expect(list.pagination.pageIndex).toBe(1)
    expect(siteMessagesApi.getPagedSiteMessages).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 10,
      isRead: false,
    })
  })

  it('updates current-page rows locally after a single message is read', () => {
    const list = useSiteMessagesList()
    list.messages.value = [
      {
        id: 'message-1',
        userId: 'user-1',
        title: '通知',
        contentPreview: 'Preview',
        isRead: false,
        createdAt: '2026-05-01T00:00:00Z',
        readAt: null,
      },
    ]

    list.markMessageReadLocal('message-1', '2026-05-06T08:00:00Z')

    expect(list.messages.value[0]).toEqual(
      expect.objectContaining({
        isRead: true,
        readAt: '2026-05-06T08:00:00Z',
      }),
    )
  })

  it('removes rows from the unread list after they are marked as read', () => {
    const list = useSiteMessagesList()
    list.filters.readStatus = 'unread'
    list.totalCount.value = 1
    list.messages.value = [
      {
        id: 'message-1',
        userId: 'user-1',
        title: '通知',
        contentPreview: 'Preview',
        isRead: false,
        createdAt: '2026-05-01T00:00:00Z',
        readAt: null,
      },
    ]

    list.markMessageReadLocal('message-1', '2026-05-06T08:00:00Z')

    expect(list.messages.value).toEqual([])
    expect(list.totalCount.value).toBe(0)
  })

  it('clears stale list data and shows an error message when loading fails', async () => {
    siteMessagesApi.getPagedSiteMessages.mockRejectedValue(new Error('request failed'))

    const list = useSiteMessagesList()
    list.messages.value = [
      {
        id: 'message-1',
        userId: 'user-1',
        title: '旧通知',
        contentPreview: 'Preview',
        isRead: false,
        createdAt: '2026-05-01T00:00:00Z',
        readAt: null,
      },
    ]
    list.totalCount.value = 1

    await list.loadMessages()

    expect(list.messages.value).toEqual([])
    expect(list.totalCount.value).toBe(0)
    expect(message.error).toHaveBeenCalledWith('request failed')
  })
})
