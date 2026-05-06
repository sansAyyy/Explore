import { describe, expect, it } from 'vitest'

import {
  buildGetPagedSiteMessagesQuery,
  createDefaultSiteMessagesFilters,
  createDefaultSiteMessagesPagination,
  formatSiteMessageTitle,
} from '@/features/site-messages/models/siteMessagesList'

describe('siteMessagesList', () => {
  it('creates the default filters and pagination state', () => {
    expect(createDefaultSiteMessagesFilters()).toEqual({
      readStatus: 'all',
    })
    expect(createDefaultSiteMessagesPagination()).toEqual({
      pageIndex: 1,
      pageSize: 10,
    })
  })

  it('maps unread and read status filters to the API query', () => {
    expect(
      buildGetPagedSiteMessagesQuery(
        { readStatus: 'unread' },
        { pageIndex: 2, pageSize: 20 },
      ),
    ).toEqual({
      pageIndex: 2,
      pageSize: 20,
      isRead: false,
    })

    expect(
      buildGetPagedSiteMessagesQuery(
        { readStatus: 'read' },
        { pageIndex: 1, pageSize: 10 },
      ),
    ).toEqual({
      pageIndex: 1,
      pageSize: 10,
      isRead: true,
    })
  })

  it('falls back to a readable title when the message title is empty', () => {
    expect(formatSiteMessageTitle({ title: null })).toBe('未命名站内信')
    expect(formatSiteMessageTitle({ title: '  ' })).toBe('未命名站内信')
    expect(formatSiteMessageTitle({ title: '系统通知' })).toBe('系统通知')
  })
})
