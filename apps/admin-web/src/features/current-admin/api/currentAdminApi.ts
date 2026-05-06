import { buildServiceApiPath, gatewayServices } from '@/shared/api/gateway'
import { http } from '@/shared/api/http'

import type {
  ChangeCurrentAdminPasswordRequest,
  CurrentAdminResponse,
  UpdateCurrentAdminProfileRequest,
} from '@/features/current-admin/types/currentAdmin'

const adminIdentityApi = (path: string) => buildServiceApiPath(gatewayServices.adminIdentity, path)

export async function getCurrentAdmin() {
  const { data } = await http.get<CurrentAdminResponse>(adminIdentityApi('/api/admin-current-user'))
  return data
}

export async function updateCurrentAdminProfile(payload: UpdateCurrentAdminProfileRequest) {
  const { data } = await http.put<CurrentAdminResponse>(adminIdentityApi('/api/admin-current-user'), payload)
  return data
}

export async function changeCurrentAdminPassword(payload: ChangeCurrentAdminPasswordRequest) {
  await http.put(adminIdentityApi('/api/admin-current-user/password'), payload)
}
