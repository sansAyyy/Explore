import { buildServiceApiPath, gatewayServices } from '@/shared/api/gateway'
import { http } from '@/shared/api/http'
import type { PagedResult } from '@/shared/types/pagination'

import type {
  CustomerAccountBasicResponse,
  CustomerAccountDetailResponse,
  GetPagedCustomerAccountsQuery,
} from '@/features/customer-accounts/types/customerAccounts'

const customerAccountApi = (path: string) =>
  buildServiceApiPath(gatewayServices.customerAccount, path)

export async function getPagedCustomerAccounts(query: GetPagedCustomerAccountsQuery) {
  const { data } = await http.get<PagedResult<CustomerAccountBasicResponse>>(
    customerAccountApi('/api/admin-customers'),
    {
      params: query,
    },
  )

  return data
}

export async function getCustomerAccountById(id: string) {
  const { data } = await http.get<CustomerAccountDetailResponse>(
    customerAccountApi(`/api/admin-customers/${id}`),
  )

  return data
}

export async function enableCustomerAccount(id: string) {
  await http.put(customerAccountApi(`/api/admin-customers/${id}/enable`))
}

export async function disableCustomerAccount(id: string) {
  await http.put(customerAccountApi(`/api/admin-customers/${id}/disable`))
}
