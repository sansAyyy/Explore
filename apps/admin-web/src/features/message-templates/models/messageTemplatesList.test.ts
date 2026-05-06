import { describe, expect, it } from 'vitest'

import {
  buildGetPagedMessageTemplatesQuery,
  createDefaultMessageTemplatesFilters,
  createDefaultMessageTemplatesPagination,
  formatMessageTemplatesStatus,
  resolveNotificationChannelTypeLabel,
} from '@/features/message-templates/models/messageTemplatesList'

describe('message templates list model', () => {
  it('creates default filters and pagination', () => {
    expect(createDefaultMessageTemplatesFilters()).toEqual({
      keyword: '',
      status: 'all',
    })
    expect(createDefaultMessageTemplatesPagination()).toEqual({
      pageIndex: 1,
      pageSize: 10,
    })
  })

  it('builds paged query with trimmed keyword and enabled status', () => {
    expect(
      buildGetPagedMessageTemplatesQuery(
        {
          keyword: '  order  ',
          status: 'enabled',
        },
        {
          pageIndex: 2,
          pageSize: 20,
        },
      ),
    ).toEqual({
      pageIndex: 2,
      pageSize: 20,
      keyword: 'order',
      isEnabled: true,
    })
  })

  it('formats status and channel labels', () => {
    expect(formatMessageTemplatesStatus(true)).toBe('启用')
    expect(formatMessageTemplatesStatus(false)).toBe('禁用')
    expect(resolveNotificationChannelTypeLabel(1)).toBe('短信')
    expect(resolveNotificationChannelTypeLabel(2)).toBe('小程序')
    expect(resolveNotificationChannelTypeLabel(3)).toBe('站内信')
  })
})
