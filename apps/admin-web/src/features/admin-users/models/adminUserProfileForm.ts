import type { FormItemRule } from 'element-plus'

import type {
  AdminUserBasicResponse,
  AdminUserCreateFormModel,
  AdminUserProfileFormModel,
  CreateAdminUserRequest,
  UpdateAdminUserRequest,
} from '@/features/admin-users/types/adminUsers'

export const ADMIN_USER_PHONE_PATTERN = /^1\d{10}$/

export function createDefaultAdminUserProfileForm(): AdminUserProfileFormModel {
  return {
    userName: '',
    email: '',
    displayName: '',
    phoneNumber: '',
  }
}

export function createDefaultAdminUserCreateForm(): AdminUserCreateFormModel {
  return {
    ...createDefaultAdminUserProfileForm(),
    password: '',
    isActive: true,
  }
}

export function createAdminUserProfileFormFromUser(
  user: Pick<AdminUserBasicResponse, 'userName' | 'email' | 'displayName' | 'phoneNumber'>,
): AdminUserProfileFormModel {
  return {
    userName: user.userName,
    email: user.email,
    displayName: user.displayName,
    phoneNumber: user.phoneNumber ?? '',
  }
}

export function buildAdminUserProfilePayload(
  form: AdminUserProfileFormModel,
): UpdateAdminUserRequest {
  const phoneNumber = form.phoneNumber.trim()

  return {
    userName: form.userName.trim(),
    email: form.email.trim(),
    displayName: form.displayName.trim(),
    phoneNumber: phoneNumber ? phoneNumber : null,
  }
}

export function buildCreateAdminUserPayload(
  form: AdminUserCreateFormModel,
): CreateAdminUserRequest {
  return {
    ...buildAdminUserProfilePayload(form),
    password: form.password.trim(),
    isActive: form.isActive,
  }
}

function createRequiredTrimmedValidator(
  requiredMessage: string,
  maxLength: number,
  maxLengthMessage: string,
) {
  return (_rule: unknown, value: string, callback: (error?: Error) => void) => {
    const length = value.trim().length

    if (length === 0) {
      callback(new Error(requiredMessage))
      return
    }

    if (length > maxLength) {
      callback(new Error(maxLengthMessage))
      return
    }

    callback()
  }
}

function validatePhoneNumber(_rule: unknown, value: string, callback: (error?: Error) => void) {
  const phoneNumber = value.trim()

  if (!phoneNumber) {
    callback()
    return
  }

  if (phoneNumber.length > 32) {
    callback(new Error('手机号长度不能超过 32 个字符'))
    return
  }

  if (!ADMIN_USER_PHONE_PATTERN.test(phoneNumber)) {
    callback(new Error('请输入有效的大陆手机号'))
    return
  }

  callback()
}

export const adminUserProfileRules = {
  userName: [
    { required: true, message: '请输入账号', trigger: 'blur' },
    {
      validator: createRequiredTrimmedValidator(
        '请输入账号',
        64,
        '账号长度不能超过 64 个字符',
      ),
      trigger: 'blur',
    },
  ],
  email: [
    { required: true, message: '请输入邮箱', trigger: 'blur' },
    {
      type: 'email',
      message: '邮箱格式不正确',
      trigger: ['blur', 'change'],
    },
    {
      validator: (_rule: unknown, value: string, callback: (error?: Error) => void) => {
        if (value.trim().length > 256) {
          callback(new Error('邮箱长度不能超过 256 个字符'))
          return
        }

        callback()
      },
      trigger: 'blur',
    },
  ],
  displayName: [
    { required: true, message: '请输入显示名称', trigger: 'blur' },
    {
      validator: createRequiredTrimmedValidator(
        '请输入显示名称',
        128,
        '显示名称长度不能超过 128 个字符',
      ),
      trigger: 'blur',
    },
  ],
  phoneNumber: [
    {
      validator: validatePhoneNumber,
      trigger: 'blur',
    },
  ],
} satisfies Record<keyof AdminUserProfileFormModel, FormItemRule[]>
