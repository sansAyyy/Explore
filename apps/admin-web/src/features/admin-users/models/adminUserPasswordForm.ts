import type {
  AdminUserChangePasswordFormModel,
  ChangeAdminUserPasswordRequest,
} from '@/features/admin-users/types/adminUsers'

export function createDefaultAdminUserChangePasswordForm(): AdminUserChangePasswordFormModel {
  return {
    newPassword: '',
    confirmPassword: '',
  }
}

export function buildChangeAdminUserPasswordPayload(
  form: AdminUserChangePasswordFormModel,
): ChangeAdminUserPasswordRequest {
  return {
    newPassword: form.newPassword.trim(),
  }
}

export function isAdminUserPasswordConfirmationMatched(
  form: Pick<AdminUserChangePasswordFormModel, 'newPassword' | 'confirmPassword'>,
) {
  return form.newPassword.trim() === form.confirmPassword.trim()
}
