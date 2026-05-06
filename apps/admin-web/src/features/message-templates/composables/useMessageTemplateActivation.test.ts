import { beforeEach, describe, expect, it, vi } from 'vitest'

import { useMessageTemplateActivation } from '@/features/message-templates/composables/useMessageTemplateActivation'
import type { MessageTemplateBasicResponse } from '@/features/message-templates/types/messageTemplates'

const messageTemplatesApi = vi.hoisted(() => ({
  disableMessageTemplate: vi.fn(),
  enableMessageTemplate: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
  success: vi.fn(),
}))

vi.mock('@/features/message-templates/api/messageTemplatesApi', () => messageTemplatesApi)
vi.mock('element-plus', () => ({
  ElMessage: message,
}))

describe('useMessageTemplateActivation', () => {
  beforeEach(() => {
    messageTemplatesApi.disableMessageTemplate.mockReset()
    messageTemplatesApi.enableMessageTemplate.mockReset()
    message.error.mockReset()
    message.success.mockReset()
  })

  it('enables a template and refreshes the list', async () => {
    messageTemplatesApi.enableMessageTemplate.mockResolvedValue(undefined)
    const refreshTemplates = vi.fn().mockResolvedValue(undefined)
    const activation = useMessageTemplateActivation({
      refreshTemplates,
    })

    const row: MessageTemplateBasicResponse = {
      id: 'template-2',
      code: 'order.paid.site_message',
      name: 'Paid Notice',
      description: 'Notify user after order payment succeeds',
      isEnabled: false,
      channelType: 3,
    }

    const promise = activation.handleEnable(row)
    expect(activation.isRowActivationPending(row.id, 'enable')).toBe(true)
    await promise

    expect(messageTemplatesApi.enableMessageTemplate).toHaveBeenCalledWith('template-2')
    expect(refreshTemplates).toHaveBeenCalledTimes(1)
    expect(message.success).toHaveBeenCalledWith('启用消息模板成功')
    expect(activation.isRowActivationPending(row.id)).toBe(false)
  })

  it('derives row-level activation availability from template state', () => {
    const activation = useMessageTemplateActivation({
      refreshTemplates: vi.fn(),
    })

    expect(
      activation.canEnableRow({
        id: 'template-2',
        code: 'order.paid.sms',
        name: 'Paid Notice',
        description: null,
        isEnabled: false,
        channelType: 1,
      }),
    ).toBe(true)

    expect(
      activation.canDisableRow({
        id: 'template-2',
        code: 'order.paid.site_message',
        name: 'Paid Notice',
        description: null,
        isEnabled: true,
        channelType: 3,
      }),
    ).toBe(true)
  })

  it('shows an error and clears pending state when activation fails', async () => {
    messageTemplatesApi.disableMessageTemplate.mockRejectedValue(new Error('request failed'))
    const refreshTemplates = vi.fn()
    const activation = useMessageTemplateActivation({
      refreshTemplates,
    })

    const row: MessageTemplateBasicResponse = {
      id: 'template-3',
      code: 'legacy.notice.sms',
      name: 'Legacy Template',
      description: null,
      isEnabled: true,
      channelType: 1,
    }

    await activation.handleDisable(row)

    expect(refreshTemplates).not.toHaveBeenCalled()
    expect(message.error).toHaveBeenCalledWith('request failed')
    expect(activation.isRowActivationPending(row.id)).toBe(false)
  })
})
