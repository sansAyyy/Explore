import { buildServiceApiPath, gatewayServices } from '@/shared/api/gateway'
import { http } from '@/shared/api/http'
import type { PagedResult } from '@/shared/types/pagination'

import type {
  CreateMessageTemplateRequest,
  GetPagedMessageTemplatesQuery,
  MessageTemplateBasicResponse,
  MessageTemplateDetailResponse,
  UpdateMessageTemplateRequest,
} from '@/features/message-templates/types/messageTemplates'

const messageCenterApi = (path: string) => buildServiceApiPath(gatewayServices.messageCenter, path)

export async function getPagedMessageTemplates(query: GetPagedMessageTemplatesQuery) {
  const { data } = await http.get<PagedResult<MessageTemplateBasicResponse>>(
    messageCenterApi('/api/message-templates'),
    {
      params: query,
    },
  )

  return data
}

export async function getMessageTemplateById(id: string) {
  const { data } = await http.get<MessageTemplateDetailResponse>(
    messageCenterApi(`/api/message-templates/${id}`),
  )

  return data
}

export async function createMessageTemplate(payload: CreateMessageTemplateRequest) {
  const { data } = await http.post<MessageTemplateDetailResponse>(
    messageCenterApi('/api/message-templates'),
    payload,
  )

  return data
}

export async function updateMessageTemplate(id: string, payload: UpdateMessageTemplateRequest) {
  const { data } = await http.put<MessageTemplateDetailResponse>(
    messageCenterApi(`/api/message-templates/${id}`),
    payload,
  )

  return data
}

export async function enableMessageTemplate(id: string) {
  await http.put(messageCenterApi(`/api/message-templates/${id}/enable`))
}

export async function disableMessageTemplate(id: string) {
  await http.put(messageCenterApi(`/api/message-templates/${id}/disable`))
}
