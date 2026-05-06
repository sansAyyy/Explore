export interface AdminLoginRequest {
  account: string
  password: string
}

export interface AdminSendPhoneLoginCodeRequest {
  phoneNumber: string
}

export interface AdminPhoneLoginRequest {
  phoneNumber: string
  verificationCode: string
}

export interface AdminLogoutRequest {
  refreshToken: string
}

export interface AdminRefreshTokenRequest {
  refreshToken: string
}

export interface AdminTokenResponse {
  accessToken: string
  refreshToken: string
  tokenType: string
  expiresAt: string
  refreshTokenExpiresAt: string
}

export interface CurrentAdminAuthorizationResponse {
  userId: string
  userName: string
  displayName: string
  roleCodes: string[]
  permissionCodes: string[]
  pagePermissionCodes: string[]
  buttonPermissionCodes: string[]
}

export interface AdminAuthState {
  accessToken: string | null
  refreshToken: string | null
  tokenType: string | null
  expiresAt: string | null
  refreshTokenExpiresAt: string | null
  userId: string | null
  userName: string | null
  displayName: string | null
  roleCodes: string[]
  permissionCodes: string[]
  pagePermissionCodes: string[]
  buttonPermissionCodes: string[]
}
