export type AdminRolesStatusFilter = 'all' | 'enabled' | 'disabled'

export type AdminPermissionResourceType = 1 | 2 | 3

export interface GetPagedAdminRolesQuery {
  pageIndex: number
  pageSize: number
  keyword?: string
  isActive?: boolean
}

export interface AdminRoleBasicResponse {
  id: string
  code: string
  name: string
  description: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string | null
}

export interface AdminRolesPageFilters {
  keyword: string
  status: AdminRolesStatusFilter
}

export interface AdminRolesPagination {
  pageIndex: number
  pageSize: number
}

export interface CreateAdminRoleRequest {
  code: string
  name: string
  description: string | null
  isActive: boolean
}

export interface UpdateAdminRoleRequest {
  code: string
  name: string
  description: string | null
}

export interface AdminRoleDetailResponse {
  id: string
  code: string
  name: string
  description: string | null
  isActive: boolean
  createdAt: string
  createdBy: string
  updatedAt: string | null
  updatedBy: string | null
  version: number
}

export interface AssignedRolePermissionResponse {
  id: string
  code: string
  name: string
  resourceType: AdminPermissionResourceType
  isActive: boolean
}

export interface AdminRolePermissionsResponse {
  roleId: string
  permissions: AssignedRolePermissionResponse[]
}

export interface AssignRolePermissionsRequest {
  permissionIds: string[]
}

export interface AdminRoleProfileFormModel {
  code: string
  name: string
  description: string
}

export interface AdminRoleCreateFormModel extends AdminRoleProfileFormModel {
  isActive: boolean
}

export type AdminRoleEditFormModel = AdminRoleProfileFormModel
