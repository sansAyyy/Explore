import type {
  GetPagedMessageTemplatesQuery,
  MessageTemplateBasicResponse,
  MessageTemplatesPageFilters,
  MessageTemplatesPagination,
  MessageTemplatesStatusFilter,
  NotificationChannelType,
} from '@/features/message-templates/types/messageTemplates'

export type MessageTemplateActivationAction = 'enable' | 'disable'

export const messageTemplatesStatusOptions: Array<{
  label: string
  value: MessageTemplatesStatusFilter
}> = [
  { label: '全部状态', value: 'all' },
  { label: '启用', value: 'enabled' },
  { label: '禁用', value: 'disabled' },
]

export function createDefaultMessageTemplatesFilters(): MessageTemplatesPageFilters {
  return {
    keyword: '',
    status: 'all',
  }
}

export function createDefaultMessageTemplatesPagination(): MessageTemplatesPagination {
  return {
    pageIndex: 1,
    pageSize: 10,
  }
}

export function buildGetPagedMessageTemplatesQuery(
  filters: MessageTemplatesPageFilters,
  pagination: MessageTemplatesPagination,
): GetPagedMessageTemplatesQuery {
  const query: GetPagedMessageTemplatesQuery = {
    pageIndex: pagination.pageIndex,
    pageSize: pagination.pageSize,
  }

  const keyword = filters.keyword.trim()
  if (keyword) {
    query.keyword = keyword
  }

  if (filters.status === 'enabled') {
    query.isEnabled = true
  }

  if (filters.status === 'disabled') {
    query.isEnabled = false
  }

  return query
}

export function formatMessageTemplatesStatus(isEnabled: boolean) {
  return isEnabled ? '启用' : '禁用'
}

export function resolveMessageTemplatesStatusType(isEnabled: boolean) {
  return isEnabled ? 'success' : 'info'
}

export function resolveNotificationChannelTypeLabel(channelType: NotificationChannelType) {
  switch (channelType) {
    case 1:
      return '短信'
    case 2:
      return '小程序'
    case 3:
      return '站内信'
    default:
      return '未知渠道'
  }
}

export function formatMessageTemplateChannelLabel(
  row: Pick<MessageTemplateBasicResponse, 'channelType'>,
) {
  return resolveNotificationChannelTypeLabel(row.channelType)
}

export function canEnableMessageTemplate(template: Pick<MessageTemplateBasicResponse, 'isEnabled'>) {
  return !template.isEnabled
}

export function canDisableMessageTemplate(template: Pick<MessageTemplateBasicResponse, 'isEnabled'>) {
  return template.isEnabled
}
