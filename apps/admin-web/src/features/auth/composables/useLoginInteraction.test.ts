import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { useLoginInteraction } from '@/features/auth/composables/useLoginInteraction'

const authApi = vi.hoisted(() => ({
  sendPhoneLoginCode: vi.fn(),
}))

vi.mock('@/features/auth/api/authApi', () => authApi)

describe('useLoginInteraction', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    authApi.sendPhoneLoginCode.mockReset()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('starts countdown after sending phone code successfully', async () => {
    authApi.sendPhoneLoginCode.mockResolvedValue(undefined)

    const interaction = useLoginInteraction()
    interaction.phoneFormModel.phoneNumber = '13800138000'

    await expect(interaction.sendPhoneCode()).resolves.toBe(true)

    expect(authApi.sendPhoneLoginCode).toHaveBeenCalledWith({
      phoneNumber: '13800138000',
    })
    expect(interaction.phoneCodeCountdown.value).toBe(60)
    expect(interaction.phoneCodeButtonText.value).toBe('60s 后重试')

    await vi.advanceTimersByTimeAsync(1000)
    expect(interaction.phoneCodeCountdown.value).toBe(59)
  })

  it('prevents repeated requests during countdown', async () => {
    authApi.sendPhoneLoginCode.mockResolvedValue(undefined)

    const interaction = useLoginInteraction()
    interaction.phoneFormModel.phoneNumber = '13800138000'

    await interaction.sendPhoneCode()
    await expect(interaction.sendPhoneCode()).resolves.toBe(false)

    expect(authApi.sendPhoneLoginCode).toHaveBeenCalledTimes(1)

    await vi.advanceTimersByTimeAsync(60000)
    expect(interaction.phoneCodeCountdown.value).toBe(0)

    await interaction.sendPhoneCode()
    expect(authApi.sendPhoneLoginCode).toHaveBeenCalledTimes(2)
  })

  it('does not start countdown when sending phone code fails', async () => {
    authApi.sendPhoneLoginCode.mockRejectedValue(new Error('send failed'))

    const interaction = useLoginInteraction()
    interaction.phoneFormModel.phoneNumber = '13800138000'

    await expect(interaction.sendPhoneCode()).rejects.toThrow('send failed')

    expect(interaction.phoneCodeCountdown.value).toBe(0)
    expect(interaction.phoneCodeButtonText.value).toBe('获取验证码')
  })

  it('keeps both form models when switching modes', () => {
    const interaction = useLoginInteraction()

    interaction.accountFormModel.account = 'seed-admin'
    interaction.accountFormModel.password = 'ExamplePass@123'
    interaction.phoneFormModel.phoneNumber = '13800138000'
    interaction.phoneFormModel.verificationCode = '666666'

    interaction.setActiveMode('phone')
    interaction.setActiveMode('account')

    expect(interaction.accountFormModel.account).toBe('seed-admin')
    expect(interaction.accountFormModel.password).toBe('ExamplePass@123')
    expect(interaction.phoneFormModel.phoneNumber).toBe('13800138000')
    expect(interaction.phoneFormModel.verificationCode).toBe('666666')
  })
})
