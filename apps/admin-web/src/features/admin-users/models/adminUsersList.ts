import type {
  AdminUserBasicResponse,
  AdminUsersPageFilters,
  AdminUsersPagination,
  AdminUsersStatusFilter,
  GetPagedAdminUsersQuery,
} from '@/features/admin-users/types/adminUsers'

export type AdminUserActivationAction = 'enable' | 'disable'

export const adminUsersStatusOptions: Array<{
  label: string
  value: AdminUsersStatusFilter
}> = [
  { label: '全部状态', value: 'all' },
  { label: '启用', value: 'enabled' },
  { label: '禁用', value: 'disabled' },
]

export function createDefaultAdminUsersFilters(): AdminUsersPageFilters {
  return {
    keyword: '',
    status: 'all',
  }
}

export function createDefaultAdminUsersPagination(): AdminUsersPagination {
  return {
    pageIndex: 1,
    pageSize: 10,
  }
}

export function buildAdminUsersQuery(
  filters: AdminUsersPageFilters,
  pagination: AdminUsersPagination,
): GetPagedAdminUsersQuery {
  const query: GetPagedAdminUsersQuery = {
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

export function formatAdminUsersStatus(isActive: boolean) {
  return isActive ? '启用' : '禁用'
}

export function resolveAdminUsersStatusType(isActive: boolean) {
  return isActive ? 'success' : 'info'
}

export function canEnableAdminUser(user: Pick<AdminUserBasicResponse, 'isActive'>) {
  return !user.isActive
}

export function canDisableAdminUser(
  user: Pick<AdminUserBasicResponse, 'id' | 'isActive'>,
  currentUserId: string | null | undefined,
) {
  return user.isActive && user.id !== currentUserId
}
