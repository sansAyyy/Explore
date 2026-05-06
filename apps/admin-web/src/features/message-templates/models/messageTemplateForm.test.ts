import { describe, expect, it } from 'vitest'

import {
  buildCreateMessageTemplatePayload,
  createDefaultMessageTemplateForm,
  createMessageTemplateFormFromDetail,
  validateMessageTemplateContent,
} from '@/features/message-templates/models/messageTemplateForm'

describe('message template form model', () => {
  it('creates a default form with a single channel model', () => {
    const form = createDefaultMessageTemplateForm()

    expect(form.isEnabled).toBe(true)
    expect(form.channelType).toBe(1)
    expect(form.titleTemplate).toBe('')
    expect(form.bodyTemplate).toBe('')
  })

  it('maps detail response into single channel form state', () => {
    const form = createMessageTemplateFormFromDetail({
      id: 'template-1',
      code: 'order.pay_success.site_message',
      name: '支付成功通知',
      description: '订单支付成功后通知用户',
      isEnabled: true,
      channelType: 3,
      titleTemplate: '站内信标题',
      bodyTemplate: '站内信正文',
    })

    expect(form.channelType).toBe(3)
    expect(form.titleTemplate).toBe('站内信标题')
    expect(form.bodyTemplate).toBe('站内信正文')
  })

  it('builds payload from single channel form state', () => {
    const form = createDefaultMessageTemplateForm()
    form.code = 'order.pay_success.sms'
    form.name = '支付成功通知'
    form.description = '  订单支付成功后通知用户  '
    form.channelType = 1
    form.titleTemplate = '  '
    form.bodyTemplate = '短信正文'

    expect(buildCreateMessageTemplatePayload(form)).toEqual({
      code: 'order.pay_success.sms',
      name: '支付成功通知',
      description: '订单支付成功后通知用户',
      isEnabled: true,
      channelType: 1,
      titleTemplate: null,
      bodyTemplate: '短信正文',
    })
  })

  it('validates single channel content rules', () => {
    const form = createDefaultMessageTemplateForm()
    expect(validateMessageTemplateContent(form)).toBe('正文模板不能为空')

    form.bodyTemplate = '正文'
    form.titleTemplate = 'x'.repeat(257)

    expect(validateMessageTemplateContent(form)).toBe('标题模板长度不能超过 256 个字符')
  })
})
