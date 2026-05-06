import type {
  ChangeCurrentAdminPasswordRequest,
  CurrentAdminPasswordFormModel,
} from '@/features/current-admin/types/currentAdmin'

export const CURRENT_ADMIN_MIN_PASSWORD_LENGTH = 8

export function createDefaultCurrentAdminPasswordForm(): CurrentAdminPasswordFormModel {
  return {
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  }
}

export function buildCurrentAdminPasswordPayload(
  form: CurrentAdminPasswordFormModel,
): ChangeCurrentAdminPasswordRequest {
  return {
    currentPassword: form.currentPassword.trim(),
    newPassword: form.newPassword.trim(),
  }
}

export function isCurrentAdminPasswordConfirmationMatched(
  form: Pick<CurrentAdminPasswordFormModel, 'newPassword' | 'confirmPassword'>,
) {
  return form.newPassword.trim() === form.confirmPassword.trim()
}

export function isCurrentAdminPasswordChanged(
  form: Pick<CurrentAdminPasswordFormModel, 'currentPassword' | 'newPassword'>,
) {
  return form.currentPassword.trim() !== form.newPassword.trim()
}
