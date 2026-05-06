export interface SendPhoneLoginCodePayload {
  phoneNumber: string
}

export interface CustomerLoginPayload {
  phoneNumber: string
  verificationCode: string
}

export interface CustomerRefreshTokenPayload {
  refreshToken: string
}

export interface CustomerLogoutPayload {
  refreshToken: string
}

export interface CustomerLoginResponse {
  accessToken: string
  refreshToken: string
  tokenType: string
  expiresAt: string
  refreshTokenExpiresAt: string
}

export interface CustomerSession {
  accessToken: string
  refreshToken: string
  tokenType: string
  expiresAt: string
  refreshTokenExpiresAt: string
}

export interface CurrentCustomer {
  id: string
  phoneNumber: string
  nickName: string
  avatarUrl: string | null
  email: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string | null
  lastLoginAt: string | null
  version: number
}

export interface UpdateCurrentCustomerProfilePayload {
  nickName: string
  avatarUrl?: string | null
  email?: string | null
}

export interface UpdateCurrentCustomerAvatarPayload {
  avatarUrl: string | null
}
