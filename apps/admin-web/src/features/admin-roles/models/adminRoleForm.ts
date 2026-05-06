import type { FormItemRule } from 'element-plus'

import type {
  AdminRoleBasicResponse,
  AdminRoleCreateFormModel,
  AdminRoleProfileFormModel,
  CreateAdminRoleRequest,
  UpdateAdminRoleRequest,
} from '@/features/admin-roles/types/adminRoles'

export const ADMIN_ROLE_CODE_PATTERN = /^[a-z0-9_.]+$/

export function createDefaultAdminRoleProfileForm(): AdminRoleProfileFormModel {
  return {
    code: '',
    name: '',
    description: '',
  }
}

export function createDefaultAdminRoleCreateForm(): AdminRoleCreateFormModel {
  return {
    ...createDefaultAdminRoleProfileForm(),
    isActive: true,
  }
}

export function createAdminRoleProfileFormFromRole(
  role: Pick<AdminRoleBasicResponse, 'code' | 'name' | 'description'>,
): AdminRoleProfileFormModel {
  return {
    code: role.code,
    name: role.name,
    description: role.description ?? '',
  }
}

export function buildAdminRoleProfilePayload(
  form: AdminRoleProfileFormModel,
): UpdateAdminRoleRequest {
  const description = form.description.trim()

  return {
    code: form.code.trim(),
    name: form.name.trim(),
    description: description ? description : null,
  }
}

export function buildCreateAdminRolePayload(
  form: AdminRoleCreateFormModel,
): CreateAdminRoleRequest {
  return {
    ...buildAdminRoleProfilePayload(form),
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

function validateRoleCode(_rule: unknown, value: string, callback: (error?: Error) => void) {
  const code = value.trim()

  if (!code) {
    callback(new Error('请输入角色编码'))
    return
  }

  if (code.length > 128) {
    callback(new Error('角色编码长度不能超过 128 个字符'))
    return
  }

  if (!ADMIN_ROLE_CODE_PATTERN.test(code)) {
    callback(new Error('角色编码仅支持小写字母、数字、下划线和点'))
    return
  }

  callback()
}

function validateDescription(_rule: unknown, value: string, callback: (error?: Error) => void) {
  if (value.trim().length > 256) {
    callback(new Error('角色描述长度不能超过 256 个字符'))
    return
  }

  callback()
}

export const adminRoleProfileRules = {
  code: [
    { required: true, message: '请输入角色编码', trigger: 'blur' },
    {
      validator: validateRoleCode,
      trigger: 'blur',
    },
  ],
  name: [
    { required: true, message: '请输入角色名称', trigger: 'blur' },
    {
      validator: createRequiredTrimmedValidator(
        '请输入角色名称',
        128,
        '角色名称长度不能超过 128 个字符',
      ),
      trigger: 'blur',
    },
  ],
  description: [
    {
      validator: validateDescription,
      trigger: 'blur',
    },
  ],
} satisfies Record<keyof AdminRoleProfileFormModel, FormItemRule[]>
