import type { FormItemRule } from 'element-plus'

import type {
  CurrentAdminProfileFormModel,
  CurrentAdminResponse,
  UpdateCurrentAdminProfileRequest,
} from '@/features/current-admin/types/currentAdmin'

export function createDefaultCurrentAdminProfileForm(): CurrentAdminProfileFormModel {
  return {
    userName: '',
    email: '',
    displayName: '',
  }
}

export function createCurrentAdminProfileFormFromResponse(
  profile: Pick<CurrentAdminResponse, 'userName' | 'email' | 'displayName'>,
): CurrentAdminProfileFormModel {
  return {
    userName: profile.userName,
    email: profile.email,
    displayName: profile.displayName,
  }
}

export function buildCurrentAdminProfilePayload(
  form: CurrentAdminProfileFormModel,
): UpdateCurrentAdminProfileRequest {
  return {
    userName: form.userName.trim(),
    email: form.email.trim(),
    displayName: form.displayName.trim(),
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

export const currentAdminProfileRules = {
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
} satisfies Record<keyof CurrentAdminProfileFormModel, FormItemRule[]>
