export interface PhoneLoginFormValues {
  phoneNumber: string
  verificationCode: string
}

export function createEmptyPhoneLoginForm(): PhoneLoginFormValues {
  return {
    phoneNumber: '',
    verificationCode: '',
  }
}

export function validatePhoneNumber(phoneNumber: string) {
  if (!phoneNumber) {
    return '请输入手机号'
  }

  if (!/^1\d{10}$/.test(phoneNumber)) {
    return '请输入正确的 11 位手机号'
  }

  return ''
}

export function validateVerificationCode(verificationCode: string) {
  if (!verificationCode) {
    return '请输入验证码'
  }

  if (!/^\d{6}$/.test(verificationCode)) {
    return '请输入 6 位验证码'
  }

  return ''
}
