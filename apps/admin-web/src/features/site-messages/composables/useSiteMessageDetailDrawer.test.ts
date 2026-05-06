import { beforeEach, describe, expect, it, vi } from 'vitest'

import { useSiteMessageDetailDrawer } from '@/features/site-messages/composables/useSiteMessageDetailDrawer'

const siteMessagesApi = vi.hoisted(() => ({
  getSiteMessageById: vi.fn(),
  markSiteMessageRead: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
}))

vi.mock('@/features/site-messages/api/siteMessagesApi', () => siteMessagesApi)
vi.mock('element-plus', () => ({
  ElMessage: message,
}))

describe('useSiteMessageDetailDrawer', () => {
  beforeEach(() => {
    siteMessagesApi.getSiteMessageById.mockReset()
    siteMessagesApi.markSiteMessageRead.mockReset()
    message.error.mockReset()
  })

  it('marks unread messages as read before loading the detail payload', async () => {
    const onMessageRead = vi.fn()
    siteMessagesApi.markSiteMessageRead.mockResolvedValue(undefined)
    siteMessagesApi.getSiteMessageById.mockResolvedValue({
      id: 'message-1',
      dispatchId: 'dispatch-1',
      userId: 'user-1',
      title: '系统通知',
      content: '完整正文',
      isRead: true,
      createdAt: '2026-05-01T00:00:00Z',
      readAt: '2026-05-06T08:00:00Z',
    })

    const drawer = useSiteMessageDetailDrawer({
      onMessageRead,
    })

    await drawer.handleOpenDetailDrawer({
      id: 'message-1',
      userId: 'user-1',
      title: '系统通知',
      contentPreview: '预览',
      isRead: false,
      createdAt: '2026-05-01T00:00:00Z',
      readAt: null,
    })

    expect(siteMessagesApi.markSiteMessageRead).toHaveBeenCalledWith('message-1')
    expect(siteMessagesApi.getSiteMessageById).toHaveBeenCalledWith('message-1')
    expect(onMessageRead).toHaveBeenCalledWith('message-1', '2026-05-06T08:00:00Z')
    expect(drawer.detail.value?.content).toBe('完整正文')
  })

  it('still marks the message read when opening by id only', async () => {
    siteMessagesApi.markSiteMessageRead.mockResolvedValue(undefined)
    siteMessagesApi.getSiteMessageById.mockResolvedValue({
      id: 'message-2',
      dispatchId: 'dispatch-2',
      userId: 'user-1',
      title: null,
      content: '完整正文',
      isRead: true,
      createdAt: '2026-05-01T00:00:00Z',
      readAt: '2026-05-06T08:00:00Z',
    })

    const drawer = useSiteMessageDetailDrawer()
    await drawer.handleOpenDetailDrawerById('message-2')

    expect(siteMessagesApi.markSiteMessageRead).toHaveBeenCalledWith('message-2')
    expect(drawer.isDetailDrawerOpen.value).toBe(true)
  })

  it('shows an error and closes the drawer when loading fails', async () => {
    siteMessagesApi.markSiteMessageRead.mockRejectedValue(new Error('request failed'))

    const drawer = useSiteMessageDetailDrawer()
    await drawer.handleOpenDetailDrawerById('message-3')

    expect(message.error).toHaveBeenCalledWith('request failed')
    expect(drawer.isDetailDrawerOpen.value).toBe(false)
    expect(drawer.detail.value).toBeNull()
  })
})
