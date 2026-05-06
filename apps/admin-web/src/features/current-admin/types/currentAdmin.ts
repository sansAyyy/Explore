export interface CurrentAdminResponse {
  id: string
  userName: string
  email: string
  phoneNumber: string | null
  displayName: string
  isActive: boolean
  createdAt: string
  updatedAt: string | null
  lastLoginAt: string | null
  version: number
}

export interface UpdateCurrentAdminProfileRequest {
  userName: string
  email: string
  displayName: string
}

export interface ChangeCurrentAdminPasswordRequest {
  currentPassword: string
  newPassword: string
}

export interface CurrentAdminProfileFormModel {
  userName: string
  email: string
  displayName: string
}

export interface CurrentAdminPasswordFormModel {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}
