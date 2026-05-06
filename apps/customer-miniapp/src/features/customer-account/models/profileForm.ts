import type {
  CurrentCustomer,
  UpdateCurrentCustomerProfilePayload,
} from '@/features/customer-account/types/customerAccount'

export interface ProfileFormValues {
  nickName: string
  email: string
}

export function createEmptyProfileForm(): ProfileFormValues {
  return {
    nickName: '',
    email: '',
  }
}

export function createProfileFormFromCustomer(customer: CurrentCustomer): ProfileFormValues {
  return {
    nickName: customer.nickName || '',
    email: customer.email || '',
  }
}

export function validateProfileForm(form: ProfileFormValues) {
  const nickName = form.nickName.trim()
  const email = form.email.trim()

  if (!nickName) {
    return '昵称不能为空'
  }

  if (nickName.length > 64) {
    return '昵称不能超过 64 个字符'
  }

  if (email.length > 0 && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    return '请输入正确的邮箱格式'
  }

  return ''
}

export function toUpdateCurrentCustomerProfilePayload(
  form: ProfileFormValues,
): UpdateCurrentCustomerProfilePayload {
  return {
    nickName: form.nickName.trim(),
    email: normalizeOptionalField(form.email),
  }
}

function normalizeOptionalField(value: string) {
  const trimmed = value.trim()
  return trimmed || null
}
