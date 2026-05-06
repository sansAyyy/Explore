import type {
  CustomerAccountBasicResponse,
  CustomerAccountsPageFilters,
  CustomerAccountsPagination,
  CustomerAccountsStatusFilter,
  GetPagedCustomerAccountsQuery,
} from '@/features/customer-accounts/types/customerAccounts'

export type CustomerAccountActivationAction = 'enable' | 'disable'

export const customerAccountsStatusOptions: Array<{
  label: string
  value: CustomerAccountsStatusFilter
}> = [
  { label: '全部状态', value: 'all' },
  { label: '启用', value: 'enabled' },
  { label: '禁用', value: 'disabled' },
]

export function createDefaultCustomerAccountsFilters(): CustomerAccountsPageFilters {
  return {
    keyword: '',
    status: 'all',
  }
}

export function createDefaultCustomerAccountsPagination(): CustomerAccountsPagination {
  return {
    pageIndex: 1,
    pageSize: 10,
  }
}

export function buildCustomerAccountsQuery(
  filters: CustomerAccountsPageFilters,
  pagination: CustomerAccountsPagination,
): GetPagedCustomerAccountsQuery {
  const query: GetPagedCustomerAccountsQuery = {
    pageIndex: pagination.pageIndex,
    pageSize: pagination.pageSize,
  }

  const keyword = filters.keyword.trim()
  if (keyword) {
    query.keyword = keyword
  }

  if (filters.status === 'enabled') {
    query.isActive = true
  }

  if (filters.status === 'disabled') {
    query.isActive = false
  }

  return query
}

export function formatCustomerAccountsStatus(isActive: boolean) {
  return isActive ? '启用' : '禁用'
}

export function resolveCustomerAccountsStatusType(isActive: boolean) {
  return isActive ? 'success' : 'info'
}

export function canEnableCustomerAccount(
  customer: Pick<CustomerAccountBasicResponse, 'isActive'>,
) {
  return !customer.isActive
}

export function canDisableCustomerAccount(
  customer: Pick<CustomerAccountBasicResponse, 'isActive'>,
) {
  return customer.isActive
}
