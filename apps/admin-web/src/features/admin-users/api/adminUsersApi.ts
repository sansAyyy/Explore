import { buildServiceApiPath, gatewayServices } from '@/shared/api/gateway'
import { http } from '@/shared/api/http'
import type { PagedResult } from '@/shared/types/pagination'

import type {
  AdminUserBasicResponse,
  AdminUserRolesResponse,
  AssignAdminUserRolesRequest,
  ChangeAdminUserPasswordRequest,
  AdminUserDetailResponse,
  CreateAdminUserRequest,
  GetSelectableAdminRolesQuery,
  GetPagedAdminUsersQuery,
  SelectableAdminRole,
  UpdateAdminUserRequest,
} from '@/features/admin-users/types/adminUsers'

const adminIdentityApi = (path: string) => buildServiceApiPath(gatewayServices.adminIdentity, path)

export async function getPagedAdminUsers(query: GetPagedAdminUsersQuery) {
  const { data } = await http.get<PagedResult<AdminUserBasicResponse>>(adminIdentityApi('/api/admin-users'), {
    params: query,
  })

  return data
}

export async function createAdminUser(payload: CreateAdminUserRequest) {
  const { data } = await http.post<AdminUserDetailResponse>(adminIdentityApi('/api/admin-users'), payload)

  return data
}

export async function updateAdminUser(id: string, payload: UpdateAdminUserRequest) {
  const { data } = await http.put<AdminUserDetailResponse>(
    adminIdentityApi(`/api/admin-users/${id}`),
    payload,
  )

  return data
}

export async function enableAdminUser(id: string) {
  await http.put(adminIdentityApi(`/api/admin-users/${id}/enable`))
}

export async function disableAdminUser(id: string) {
  await http.put(adminIdentityApi(`/api/admin-users/${id}/disable`))
}

export async function changeAdminUserPassword(
  id: string,
  payload: ChangeAdminUserPasswordRequest,
) {
  await http.put(adminIdentityApi(`/api/admin-users/${id}/password`), payload)
}

export async function getAdminUserRoles(userId: string) {
  const { data } = await http.get<AdminUserRolesResponse>(
    adminIdentityApi(`/api/admin-users/${userId}/roles`),
  )

  return data
}

export async function assignAdminUserRoles(
  userId: string,
  payload: AssignAdminUserRolesRequest,
) {
  const { data } = await http.put<AdminUserRolesResponse>(
    adminIdentityApi(`/api/admin-users/${userId}/roles`),
    payload,
  )

  return data
}

export async function getSelectableAdminRoles(query: GetSelectableAdminRolesQuery) {
  const { data } = await http.get<PagedResult<SelectableAdminRole>>(
    adminIdentityApi('/api/admin-roles'),
    {
      params: query,
    },
  )

  return data
}
