export type AdminUsersStatusFilter = 'all' | 'enabled' | 'disabled'

export interface GetPagedAdminUsersQuery {
  pageIndex: number
  pageSize: number
  keyword?: string
  isActive?: boolean
}

export interface AdminUserBasicResponse {
  id: string
  userName: string
  email: string
  phoneNumber: string | null
  displayName: string
  isActive: boolean
  createdAt: string
  updatedAt: string | null
  lastLoginAt: string | null
}

export interface AdminUsersPageFilters {
  keyword: string
  status: AdminUsersStatusFilter
}

export interface AdminUsersPagination {
  pageIndex: number
  pageSize: number
}

export interface CreateAdminUserRequest {
  userName: string
  email: string
  displayName: string
  phoneNumber: string | null
  password: string
  isActive: boolean
}

export interface UpdateAdminUserRequest {
  userName: string
  email: string
  displayName: string
  phoneNumber: string | null
}

export interface ChangeAdminUserPasswordRequest {
  newPassword: string
}

export interface GetSelectableAdminRolesQuery {
  pageIndex: number
  pageSize: number
  keyword?: string
  isActive?: boolean
}

export interface AssignAdminUserRolesRequest {
  roleIds: string[]
}

export interface AdminUserDetailResponse {
  id: string
  userName: string
  email: string
  phoneNumber: string | null
  displayName: string
  isActive: boolean
  createdAt: string
  createdBy: string
  updatedAt: string | null
  updatedBy: string | null
  lastLoginAt: string | null
  version: number
}

export interface SelectableAdminRole {
  id: string
  code: string
  name: string
  description: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string | null
}

export interface AssignedUserRoleResponse {
  id: string
  code: string
  name: string
  isActive: boolean
}

export interface AdminUserRolesResponse {
  adminUserId: string
  roles: AssignedUserRoleResponse[]
}

export interface AdminUserProfileFormModel {
  userName: string
  email: string
  displayName: string
  phoneNumber: string
}

export interface AdminUserCreateFormModel extends AdminUserProfileFormModel {
  password: string
  isActive: boolean
}

export type AdminUserEditFormModel = AdminUserProfileFormModel

export interface AdminUserChangePasswordFormModel {
  newPassword: string
  confirmPassword: string
}

export interface AdminUserRoleSearchFilters {
  keyword: string
}

export interface SelectableAdminRoleOption {
  value: string
  label: string
  code: string
}
