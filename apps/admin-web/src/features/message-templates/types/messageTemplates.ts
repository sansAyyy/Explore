export type MessageTemplatesStatusFilter = 'all' | 'enabled' | 'disabled'

export type NotificationChannelType = 1 | 2 | 3

export interface GetPagedMessageTemplatesQuery {
  pageIndex: number
  pageSize: number
  keyword?: string
  isEnabled?: boolean
}

export interface MessageTemplateBasicResponse {
  id: string
  code: string
  name: string
  description: string | null
  isEnabled: boolean
  channelType: NotificationChannelType
}

export interface MessageTemplateDetailResponse {
  id: string
  code: string
  name: string
  description: string | null
  isEnabled: boolean
  channelType: NotificationChannelType
  titleTemplate: string | null
  bodyTemplate: string
}

export interface CreateMessageTemplateRequest {
  code: string
  name: string
  description: string | null
  isEnabled: boolean
  channelType: NotificationChannelType
  titleTemplate: string | null
  bodyTemplate: string
}

export interface UpdateMessageTemplateRequest {
  code: string
  name: string
  description: string | null
  isEnabled: boolean
  channelType: NotificationChannelType
  titleTemplate: string | null
  bodyTemplate: string
}

export interface MessageTemplatesPageFilters {
  keyword: string
  status: MessageTemplatesStatusFilter
}

export interface MessageTemplatesPagination {
  pageIndex: number
  pageSize: number
}

export interface MessageTemplateFormModel {
  code: string
  name: string
  description: string
  isEnabled: boolean
  channelType: NotificationChannelType
  titleTemplate: string
  bodyTemplate: string
}
