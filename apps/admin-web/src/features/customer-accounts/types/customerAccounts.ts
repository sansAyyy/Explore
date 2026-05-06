export type CustomerAccountsStatusFilter = 'all' | 'enabled' | 'disabled'

export interface GetPagedCustomerAccountsQuery {
  pageIndex: number
  pageSize: number
  keyword?: string
  isActive?: boolean
}

export interface CustomerAccountBasicResponse {
  id: string
  phoneNumber: string
  nickName: string
  avatarUrl: string | null
  email: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string | null
  lastLoginAt: string | null
}

export interface CustomerAccountDetailResponse {
  id: string
  phoneNumber: string
  nickName: string
  avatarUrl: string | null
  email: string | null
  isActive: boolean
  createdAt: string
  createdBy: string
  updatedAt: string | null
  updatedBy: string | null
  lastLoginAt: string | null
  version: number
}

export interface CustomerAccountsPageFilters {
  keyword: string
  status: CustomerAccountsStatusFilter
}

export interface CustomerAccountsPagination {
  pageIndex: number
  pageSize: number
}
