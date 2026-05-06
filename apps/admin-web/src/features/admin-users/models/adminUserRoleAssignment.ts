import type {
  AdminUserRoleSearchFilters,
  AssignedUserRoleResponse,
  AssignAdminUserRolesRequest,
  GetSelectableAdminRolesQuery,
  SelectableAdminRole,
  SelectableAdminRoleOption,
} from '@/features/admin-users/types/adminUsers'

export function createDefaultAdminUserRoleSearchFilters(): AdminUserRoleSearchFilters {
  return {
    keyword: '',
  }
}

export function buildSelectableAdminRolesQuery(
  filters: AdminUserRoleSearchFilters,
): GetSelectableAdminRolesQuery {
  const keyword = filters.keyword.trim()

  return {
    pageIndex: 1,
    pageSize: 100,
    isActive: true,
    ...(keyword ? { keyword } : {}),
  }
}

export function buildAssignAdminUserRolesPayload(roleIds: string[]): AssignAdminUserRolesRequest {
  return {
    roleIds: [...roleIds],
  }
}

export function mapSelectableAdminRolesToOptions(
  roles: SelectableAdminRole[],
): SelectableAdminRoleOption[] {
  return roles.map((role) => ({
    value: role.id,
    label: role.name,
    code: role.code,
  }))
}

export function resolveSelectedRoleIdsFromAssignments(roles: AssignedUserRoleResponse[]) {
  return roles.filter((role) => role.isActive).map((role) => role.id)
}
