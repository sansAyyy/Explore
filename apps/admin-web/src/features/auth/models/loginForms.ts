import type { FormItemRule } from 'element-plus'

import type {
  AdminLoginRequest,
  AdminPhoneLoginRequest,
  AdminSendPhoneLoginCodeRequest,
} from '@/features/auth/types/auth'

export type LoginMode = 'account' | 'phone'

export interface AccountLoginFormModel {
  account: string
  password: string
}

export interface PhoneLoginFormModel {
  phoneNumber: string
  verificationCode: string
}

export const ADMIN_PHONE_PATTERN = /^1\d{10}$/
export const PHONE_LOGIN_CODE_COUNTDOWN_SECONDS = 60
export const PHONE_LOGIN_HELPER_TEXT = '适用于已绑定手机号的管理员身份验证。'
export const PHONE_LOGIN_CODE_BUTTON_TEXT = '获取验证码'

export function createDefaultAccountLoginForm(): AccountLoginFormModel {
  return {
    account: '',
    password: '',
  }
}

export function createDefaultPhoneLoginForm(): PhoneLoginFormModel {
  return {
    phoneNumber: '',
    verificationCode: '',
  }
}

export function isValidAdminPhoneNumber(value: string) {
  return ADMIN_PHONE_PATTERN.test(value.trim())
}

export function validateAccount(value: string) {
  const trimmedValue = value.trim()

  if (!trimmedValue) {
    return '请输入账号'
  }

  if (trimmedValue.length > 256) {
    return '账号长度不能超过 256 个字符'
  }

  return null
}

export function validatePassword(value: string) {
  const trimmedValue = value.trim()

  if (!trimmedValue) {
    return '请输入密码'
  }

  if (trimmedValue.length > 128) {
    return '密码长度不能超过 128 个字符'
  }

  return null
}

export function validatePhoneNumber(value: string) {
  const trimmedValue = value.trim()

  if (!trimmedValue) {
    return '请输入手机号'
  }

  if (trimmedValue.length > 32) {
    return '手机号长度不能超过 32 个字符'
  }

  if (!isValidAdminPhoneNumber(trimmedValue)) {
    return '请输入有效的大陆手机号'
  }

  return null
}

export function validateVerificationCode(value: string) {
  const trimmedValue = value.trim()

  if (!trimmedValue) {
    return '请输入验证码'
  }

  if (trimmedValue.length > 16) {
    return '验证码长度不能超过 16 个字符'
  }

  return null
}

function createValidator(
  validator: (value: string) => string | null,
) {
  return (_rule: unknown, value: string, callback: (error?: Error) => void) => {
    const errorMessage = validator(value)

    if (errorMessage) {
      callback(new Error(errorMessage))
      return
    }

    callback()
  }
}

export function buildAccountLoginPayload(form: AccountLoginFormModel): AdminLoginRequest {
  return {
    account: form.account.trim(),
    password: form.password,
  }
}

export function buildSendPhoneLoginCodePayload(
  form: Pick<PhoneLoginFormModel, 'phoneNumber'>,
): AdminSendPhoneLoginCodeRequest {
  return {
    phoneNumber: form.phoneNumber.trim(),
  }
}

export function buildPhoneLoginPayload(form: PhoneLoginFormModel): AdminPhoneLoginRequest {
  return {
    phoneNumber: form.phoneNumber.trim(),
    verificationCode: form.verificationCode.trim(),
  }
}

export const accountLoginRules = {
  account: [
    {
      validator: createValidator(validateAccount),
      trigger: 'blur',
    },
  ],
  password: [
    {
      validator: createValidator(validatePassword),
      trigger: 'blur',
    },
  ],
} satisfies Record<keyof AccountLoginFormModel, FormItemRule[]>

export const phoneLoginRules = {
  phoneNumber: [
    {
      validator: createValidator(validatePhoneNumber),
      trigger: 'blur',
    },
  ],
  verificationCode: [
    {
      validator: createValidator(validateVerificationCode),
      trigger: 'blur',
    },
  ],
} satisfies Record<keyof PhoneLoginFormModel, FormItemRule[]>
