import { buildServiceApiPath, gatewayServices } from '@/shared/api/gateway'
import { http } from '@/shared/api/http'
import type { PagedResult } from '@/shared/types/pagination'

import type {
  AdminRoleBasicResponse,
  AdminRoleDetailResponse,
  AdminRolePermissionsResponse,
  AssignRolePermissionsRequest,
  CreateAdminRoleRequest,
  GetPagedAdminRolesQuery,
  UpdateAdminRoleRequest,
} from '@/features/admin-roles/types/adminRoles'

const adminIdentityApi = (path: string) => buildServiceApiPath(gatewayServices.adminIdentity, path)

export async function getPagedAdminRoles(query: GetPagedAdminRolesQuery) {
  const { data } = await http.get<PagedResult<AdminRoleBasicResponse>>(adminIdentityApi('/api/admin-roles'), {
    params: query,
  })

  return data
}

export async function createAdminRole(payload: CreateAdminRoleRequest) {
  const { data } = await http.post<AdminRoleDetailResponse>(adminIdentityApi('/api/admin-roles'), payload)

  return data
}

export async function getAdminRoleById(id: string) {
  const { data } = await http.get<AdminRoleDetailResponse>(adminIdentityApi(`/api/admin-roles/${id}`))

  return data
}

export async function updateAdminRole(id: string, payload: UpdateAdminRoleRequest) {
  const { data } = await http.put<AdminRoleDetailResponse>(
    adminIdentityApi(`/api/admin-roles/${id}`),
    payload,
  )

  return data
}

export async function enableAdminRole(id: string) {
  await http.put(adminIdentityApi(`/api/admin-roles/${id}/enable`))
}

export async function disableAdminRole(id: string) {
  await http.put(adminIdentityApi(`/api/admin-roles/${id}/disable`))
}

export async function deleteAdminRole(id: string) {
  await http.delete(adminIdentityApi(`/api/admin-roles/${id}`))
}

export async function getAdminRolePermissions(roleId: string) {
  const { data } = await http.get<AdminRolePermissionsResponse>(
    adminIdentityApi(`/api/admin-roles/${roleId}/permissions`),
  )

  return data
}

export async function assignAdminRolePermissions(
  roleId: string,
  payload: AssignRolePermissionsRequest,
) {
  const { data } = await http.put<AdminRolePermissionsResponse>(
    adminIdentityApi(`/api/admin-roles/${roleId}/permissions`),
    payload,
  )

  return data
}
