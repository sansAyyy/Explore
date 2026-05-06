import { buildServiceApiPath, gatewayServices } from '@/shared/api/gateway'
import { http } from '@/shared/api/http'
import type {
  AdminLoginRequest,
  AdminLogoutRequest,
  AdminPhoneLoginRequest,
  AdminRefreshTokenRequest,
  AdminSendPhoneLoginCodeRequest,
  AdminTokenResponse,
  CurrentAdminAuthorizationResponse,
} from '@/features/auth/types/auth'

const adminIdentityApi = (path: string) => buildServiceApiPath(gatewayServices.adminIdentity, path)

export async function login(payload: AdminLoginRequest) {
  const { data } = await http.post<AdminTokenResponse>(adminIdentityApi('/api/admin-auth/login'), payload, {
    _skipAuthRefresh: true,
  })
  return data
}

export async function sendPhoneLoginCode(payload: AdminSendPhoneLoginCodeRequest) {
  await http.post(adminIdentityApi('/api/admin-auth/phone/code'), payload, {
    _skipAuthRefresh: true,
  })
}

export async function phoneLogin(payload: AdminPhoneLoginRequest) {
  const { data } = await http.post<AdminTokenResponse>(
    adminIdentityApi('/api/admin-auth/phone/login'),
    payload,
    {
      _skipAuthRefresh: true,
    },
  )
  return data
}

export async function logout(payload: AdminLogoutRequest) {
  await http.post(adminIdentityApi('/api/admin-auth/logout'), payload, {
    _skipAuthRefresh: true,
  })
}

export async function refresh(payload: AdminRefreshTokenRequest) {
  const { data } = await http.post<AdminTokenResponse>(
    adminIdentityApi('/api/admin-auth/refresh'),
    payload,
    {
      _skipAuthRefresh: true,
    },
  )
  return data
}

export async function fetchCurrentAuthorization() {
  const { data } = await http.get<CurrentAdminAuthorizationResponse>(
    adminIdentityApi('/api/admin-authorization/current'),
  )
  return data
}
