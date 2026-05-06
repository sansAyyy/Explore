export type AdminPermissionsStatusFilter = 'all' | 'enabled' | 'disabled'

export type AdminPermissionResourceType = 1 | 2 | 3

export interface GetPagedAdminPermissionsQuery {
  pageIndex: number
  pageSize: number
  keyword?: string
  isActive?: boolean
  resourceType?: AdminPermissionResourceType
}

export interface GetAdminPermissionTreeQuery {
  isActive?: boolean
}

export interface AdminPermissionBasicResponse {
  id: string
  parentId: string | null
  code: string
  name: string
  description: string | null
  resourceType: AdminPermissionResourceType
  isActive: boolean
  createdAt: string
  updatedAt: string | null
}

export interface AdminPermissionDetailResponse {
  id: string
  parentId: string | null
  code: string
  name: string
  description: string | null
  resourceType: AdminPermissionResourceType
  isActive: boolean
  createdAt: string
  createdBy: string
  updatedAt: string | null
  updatedBy: string | null
  version: number
}

export interface AdminPermissionTreeNodeResponse {
  id: string
  parentId: string | null
  code: string
  name: string
  description: string | null
  resourceType: AdminPermissionResourceType
  isActive: boolean
  children: AdminPermissionTreeNodeResponse[]
}

export interface CreateAdminPermissionRequest {
  parentId: string | null
  code: string
  name: string
  description: string | null
  resourceType: AdminPermissionResourceType
  isActive: boolean
}

export interface UpdateAdminPermissionRequest {
  parentId: string | null
  code: string
  name: string
  description: string | null
  resourceType: AdminPermissionResourceType
}

export interface AdminPermissionFormModel {
  parentId: string | null
  code: string
  name: string
  description: string
  resourceType: AdminPermissionResourceType
  isActive: boolean
}

export interface AdminPermissionTreeNode {
  id: string
  parentId: string | null
  code: string
  name: string
  description: string | null
  resourceType: AdminPermissionResourceType
  isGroup: boolean
  resourceTypeLabel: string
  isActive: boolean
  children: AdminPermissionTreeNode[]
  disabled?: boolean
}

export interface AdminPermissionTreeFilterState {
  keyword: string
  resourceType: 'all' | AdminPermissionResourceType
  status: AdminPermissionsStatusFilter
  onlySelected: boolean
}
