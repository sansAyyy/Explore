import type {
  AdminRoleBasicResponse,
  AdminRolesPageFilters,
  AdminRolesPagination,
  AdminRolesStatusFilter,
  GetPagedAdminRolesQuery,
} from '@/features/admin-roles/types/adminRoles'

export type AdminRoleActivationAction = 'enable' | 'disable'

export const adminRolesStatusOptions: Array<{
  label: string
  value: AdminRolesStatusFilter
}> = [
  { label: '全部状态', value: 'all' },
  { label: '启用', value: 'enabled' },
  { label: '禁用', value: 'disabled' },
]

export function createDefaultAdminRolesFilters(): AdminRolesPageFilters {
  return {
    keyword: '',
    status: 'all',
  }
}

export function createDefaultAdminRolesPagination(): AdminRolesPagination {
  return {
    pageIndex: 1,
    pageSize: 10,
  }
}

export function buildAdminRolesQuery(
  filters: AdminRolesPageFilters,
  pagination: AdminRolesPagination,
): GetPagedAdminRolesQuery {
  const query: GetPagedAdminRolesQuery = {
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

export function formatAdminRolesStatus(isActive: boolean) {
  return isActive ? '启用' : '禁用'
}

export function resolveAdminRolesStatusType(isActive: boolean) {
  return isActive ? 'success' : 'info'
}

export function canEnableAdminRole(role: Pick<AdminRoleBasicResponse, 'isActive'>) {
  return !role.isActive
}

export function canDisableAdminRole(role: Pick<AdminRoleBasicResponse, 'isActive'>) {
  return role.isActive
}
