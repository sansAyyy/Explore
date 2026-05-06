import { buildServiceApiPath, gatewayServices } from '@/shared/api/gateway'
import { http } from '@/shared/api/http'
import type { PagedResult } from '@/shared/types/pagination'

import type {
  AdminPermissionBasicResponse,
  AdminPermissionDetailResponse,
  AdminPermissionTreeNodeResponse,
  CreateAdminPermissionRequest,
  GetAdminPermissionTreeQuery,
  GetPagedAdminPermissionsQuery,
  UpdateAdminPermissionRequest,
} from '@/features/admin-permissions/types/adminPermissions'

const adminIdentityApi = (path: string) => buildServiceApiPath(gatewayServices.adminIdentity, path)

export async function getPagedAdminPermissions(query: GetPagedAdminPermissionsQuery) {
  const { data } = await http.get<PagedResult<AdminPermissionBasicResponse>>(
    adminIdentityApi('/api/admin-permissions'),
    {
      params: query,
    },
  )

  return data
}

export async function getAdminPermissionById(id: string) {
  const { data } = await http.get<AdminPermissionDetailResponse>(
    adminIdentityApi(`/api/admin-permissions/${id}`),
  )

  return data
}

export async function getAdminPermissionRoots(query: GetAdminPermissionTreeQuery) {
  const { data } = await http.get<AdminPermissionBasicResponse[]>(
    adminIdentityApi('/api/admin-permissions/roots'),
    {
      params: query,
    },
  )

  return data
}

export async function getAdminPermissionDescendants(id: string, query: GetAdminPermissionTreeQuery) {
  const { data } = await http.get<AdminPermissionTreeNodeResponse>(
    adminIdentityApi(`/api/admin-permissions/${id}/descendants`),
    {
      params: query,
    },
  )

  return data
}

export async function createAdminPermission(payload: CreateAdminPermissionRequest) {
  const { data } = await http.post<AdminPermissionDetailResponse>(
    adminIdentityApi('/api/admin-permissions'),
    payload,
  )

  return data
}

export async function updateAdminPermission(id: string, payload: UpdateAdminPermissionRequest) {
  const { data } = await http.put<AdminPermissionDetailResponse>(
    adminIdentityApi(`/api/admin-permissions/${id}`),
    payload,
  )

  return data
}

export async function enableAdminPermission(id: string) {
  await http.put(adminIdentityApi(`/api/admin-permissions/${id}/enable`))
}

export async function disableAdminPermission(id: string) {
  await http.put(adminIdentityApi(`/api/admin-permissions/${id}/disable`))
}

export async function deleteAdminPermission(id: string) {
  await http.delete(adminIdentityApi(`/api/admin-permissions/${id}`))
}
