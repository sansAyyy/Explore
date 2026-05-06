export type SiteMessagesReadStatusFilter = 'all' | 'unread' | 'read'

export interface GetPagedSiteMessagesQuery {
  pageIndex: number
  pageSize: number
  isRead?: boolean
}

export interface SiteMessageBasicResponse {
  id: string
  userId: string
  title: string | null
  contentPreview: string
  isRead: boolean
  createdAt: string
  readAt: string | null
}

export interface SiteMessageDetailResponse {
  id: string
  dispatchId: string
  userId: string
  title: string | null
  content: string
  isRead: boolean
  createdAt: string
  readAt: string | null
}

export interface SiteMessagesPageFilters {
  readStatus: SiteMessagesReadStatusFilter
}

export interface SiteMessagesPagination {
  pageIndex: number
  pageSize: number
}
