import { describe, expect, it } from 'vitest'

import {
  buildAccountLoginPayload,
  buildPhoneLoginPayload,
  buildSendPhoneLoginCodePayload,
  isValidAdminPhoneNumber,
  validatePhoneNumber,
  validateVerificationCode,
} from '@/features/auth/models/loginForms'

describe('login forms model', () => {
  it('trims account login payload and preserves password', () => {
    expect(
      buildAccountLoginPayload({
        account: '  seed-admin  ',
        password: ' ExamplePass@123 ',
      }),
    ).toEqual({
      account: 'seed-admin',
      password: ' ExamplePass@123 ',
    })
  })

  it('trims phone login payloads', () => {
    expect(
      buildSendPhoneLoginCodePayload({
        phoneNumber: ' 13800138000 ',
      }),
    ).toEqual({
      phoneNumber: '13800138000',
    })

    expect(
      buildPhoneLoginPayload({
        phoneNumber: ' 13800138000 ',
        verificationCode: ' 666666 ',
      }),
    ).toEqual({
      phoneNumber: '13800138000',
      verificationCode: '666666',
    })
  })

  it('validates mainland phone numbers', () => {
    expect(isValidAdminPhoneNumber('13800138000')).toBe(true)
    expect(isValidAdminPhoneNumber(' 13800138000 ')).toBe(true)
    expect(isValidAdminPhoneNumber('23800138000')).toBe(false)
    expect(isValidAdminPhoneNumber('1380013800')).toBe(false)
  })

  it('returns specific phone number validation messages', () => {
    expect(validatePhoneNumber('')).toBe('请输入手机号')
    expect(validatePhoneNumber('1380013800')).toBe('请输入有效的大陆手机号')
    expect(validatePhoneNumber('1'.repeat(33))).toBe('手机号长度不能超过 32 个字符')
  })

  it('returns specific verification code validation messages', () => {
    expect(validateVerificationCode('')).toBe('请输入验证码')
    expect(validateVerificationCode(' '.repeat(4))).toBe('请输入验证码')
    expect(validateVerificationCode('1'.repeat(17))).toBe('验证码长度不能超过 16 个字符')
    expect(validateVerificationCode(' 123456 ')).toBeNull()
  })
})
