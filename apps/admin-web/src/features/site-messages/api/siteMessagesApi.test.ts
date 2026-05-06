import { beforeEach, describe, expect, it, vi } from 'vitest'

const http = vi.hoisted(() => ({
  get: vi.fn(),
  put: vi.fn(),
}))

vi.mock('@/shared/api/http', () => ({
  http,
}))

import {
  getPagedSiteMessages,
  getSiteMessageById,
  markAllSiteMessagesRead,
  markSiteMessageRead,
} from '@/features/site-messages/api/siteMessagesApi'

describe('siteMessagesApi', () => {
  beforeEach(() => {
    http.get.mockReset()
    http.put.mockReset()
  })

  it('requests the paged site message list through the message center gateway service', async () => {
    http.get.mockResolvedValue({
      data: {
        totalCount: 0,
        items: [],
      },
    })

    await getPagedSiteMessages({
      pageIndex: 1,
      pageSize: 10,
      isRead: false,
    })

    expect(http.get).toHaveBeenCalledWith('/message-center/api/site-messages', {
      params: {
        pageIndex: 1,
        pageSize: 10,
        isRead: false,
      },
    })
  })

  it('requests a single site message detail by id', async () => {
    http.get.mockResolvedValue({
      data: {
        id: 'message-1',
      },
    })

    await getSiteMessageById('message-1')

    expect(http.get).toHaveBeenCalledWith('/message-center/api/site-messages/message-1')
  })

  it('marks a single site message as read', async () => {
    http.put.mockResolvedValue({})

    await markSiteMessageRead('message-2')

    expect(http.put).toHaveBeenCalledWith('/message-center/api/site-messages/message-2/read')
  })

  it('marks all site messages as read', async () => {
    http.put.mockResolvedValue({})

    await markAllSiteMessagesRead()

    expect(http.put).toHaveBeenCalledWith('/message-center/api/site-messages/read-all')
  })
})
