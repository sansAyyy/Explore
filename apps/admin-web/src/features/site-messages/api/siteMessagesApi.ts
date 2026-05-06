import { buildServiceApiPath, gatewayServices } from '@/shared/api/gateway'
import { http } from '@/shared/api/http'
import type { PagedResult } from '@/shared/types/pagination'

import type {
  GetPagedSiteMessagesQuery,
  SiteMessageBasicResponse,
  SiteMessageDetailResponse,
} from '@/features/site-messages/types/siteMessages'

const messageCenterApi = (path: string) => buildServiceApiPath(gatewayServices.messageCenter, path)

export async function getPagedSiteMessages(query: GetPagedSiteMessagesQuery) {
  const { data } = await http.get<PagedResult<SiteMessageBasicResponse>>(
    messageCenterApi('/api/site-messages'),
    {
      params: query,
    },
  )

  return data
}

export async function getSiteMessageById(id: string) {
  const { data } = await http.get<SiteMessageDetailResponse>(messageCenterApi(`/api/site-messages/${id}`))
  return data
}

export async function markSiteMessageRead(id: string) {
  await http.put(messageCenterApi(`/api/site-messages/${id}/read`))
}

export async function markAllSiteMessagesRead() {
  await http.put(messageCenterApi('/api/site-messages/read-all'))
}
