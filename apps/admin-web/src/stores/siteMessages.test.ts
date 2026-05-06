import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

import { useSiteMessagesStore } from '@/stores/siteMessages'

const siteMessagesApi = vi.hoisted(() => ({
  getPagedSiteMessages: vi.fn(),
  markAllSiteMessagesRead: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
}))

vi.mock('@/features/site-messages/api/siteMessagesApi', () => siteMessagesApi)
vi.mock('element-plus', () => ({
  ElMessage: message,
}))

describe('siteMessages store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    siteMessagesApi.getPagedSiteMessages.mockReset()
    siteMessagesApi.markAllSiteMessagesRead.mockReset()
    message.error.mockReset()
  })

  it('refreshes the unread count from the unread-only endpoint query', async () => {
    siteMessagesApi.getPagedSiteMessages.mockResolvedValue({
      totalCount: 12,
      items: [],
    })

    const store = useSiteMessagesStore()
    await store.refreshUnreadCount()

    expect(siteMessagesApi.getPagedSiteMessages).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 1,
      isRead: false,
    })
    expect(store.unreadCount).toBe(12)
    expect(store.unreadCountBadgeValue).toBe('12')
  })

  it('loads preview context when the inbox preview drawer opens', async () => {
    siteMessagesApi.getPagedSiteMessages
      .mockResolvedValueOnce({
        totalCount: 3,
        items: [],
      })
      .mockResolvedValueOnce({
        totalCount: 5,
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

    const store = useSiteMessagesStore()
    await store.openPreviewDrawer()

    expect(store.isPreviewDrawerOpen).toBe(true)
    expect(store.unreadCount).toBe(3)
    expect(store.previewItems).toHaveLength(1)
  })

  it('marks all messages as read and refreshes preview context', async () => {
    siteMessagesApi.markAllSiteMessagesRead.mockResolvedValue(undefined)
    siteMessagesApi.getPagedSiteMessages
      .mockResolvedValueOnce({
        totalCount: 0,
        items: [],
      })
      .mockResolvedValueOnce({
        totalCount: 2,
        items: [
          {
            id: 'message-1',
            userId: 'user-1',
            title: '系统通知',
            contentPreview: 'Preview',
            isRead: true,
            createdAt: '2026-05-01T00:00:00Z',
            readAt: '2026-05-06T08:00:00Z',
          },
        ],
      })

    const store = useSiteMessagesStore()
    await store.markAllRead()

    expect(siteMessagesApi.markAllSiteMessagesRead).toHaveBeenCalledTimes(1)
    expect(store.unreadCount).toBe(0)
    expect(store.previewItems[0]?.isRead).toBe(true)
  })

  it('clears state after logout or session reset', () => {
    const store = useSiteMessagesStore()
    store.$patch({
      unreadCount: 3,
      previewItems: [
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
      isPreviewDrawerOpen: true,
    })

    store.clear()

    expect(store.unreadCount).toBe(0)
    expect(store.previewItems).toEqual([])
    expect(store.isPreviewDrawerOpen).toBe(false)
  })
})
