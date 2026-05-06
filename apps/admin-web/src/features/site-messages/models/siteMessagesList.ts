import type {
  GetPagedSiteMessagesQuery,
  SiteMessageBasicResponse,
  SiteMessagesPageFilters,
  SiteMessagesPagination,
  SiteMessagesReadStatusFilter,
} from '@/features/site-messages/types/siteMessages'

export const siteMessagesReadStatusOptions: Array<{
  label: string
  value: SiteMessagesReadStatusFilter
}> = [
  { label: '全部消息', value: 'all' },
  { label: '未读消息', value: 'unread' },
  { label: '已读消息', value: 'read' },
]

export function createDefaultSiteMessagesFilters(): SiteMessagesPageFilters {
  return {
    readStatus: 'all',
  }
}

export function createDefaultSiteMessagesPagination(): SiteMessagesPagination {
  return {
    pageIndex: 1,
    pageSize: 10,
  }
}

export function buildGetPagedSiteMessagesQuery(
  filters: SiteMessagesPageFilters,
  pagination: SiteMessagesPagination,
): GetPagedSiteMessagesQuery {
  const query: GetPagedSiteMessagesQuery = {
    pageIndex: pagination.pageIndex,
    pageSize: pagination.pageSize,
  }

  if (filters.readStatus === 'unread') {
    query.isRead = false
  }

  if (filters.readStatus === 'read') {
    query.isRead = true
  }

  return query
}

export function formatSiteMessageStatus(isRead: boolean) {
  return isRead ? '已读' : '未读'
}

export function resolveSiteMessageStatusType(isRead: boolean) {
  return isRead ? 'info' : 'primary'
}

export function formatSiteMessageTitle(row: Pick<SiteMessageBasicResponse, 'title'>) {
  const title = row.title?.trim()
  return title && title.length > 0 ? title : '未命名站内信'
}
