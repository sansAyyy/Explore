import { ElMessage } from 'element-plus'
import { reactive } from 'vue'

import {
  disableMessageTemplate,
  enableMessageTemplate,
} from '@/features/message-templates/api/messageTemplatesApi'
import {
  canDisableMessageTemplate,
  canEnableMessageTemplate,
} from '@/features/message-templates/models/messageTemplatesList'
import type { MessageTemplateActivationAction } from '@/features/message-templates/models/messageTemplatesList'
import type { MessageTemplateBasicResponse } from '@/features/message-templates/types/messageTemplates'
import { extractErrorMessage } from '@/shared/api/error'

export function useMessageTemplateActivation(options: {
  refreshTemplates: () => Promise<unknown> | void
}) {
  const activationState = reactive<{
    action: MessageTemplateActivationAction | null
    templateId: string | null
  }>({
    action: null,
    templateId: null,
  })

  function canEnableRow(row: MessageTemplateBasicResponse) {
    return canEnableMessageTemplate(row)
  }

  function canDisableRow(row: MessageTemplateBasicResponse) {
    return canDisableMessageTemplate(row)
  }

  function isRowActivationPending(templateId: string, action?: MessageTemplateActivationAction) {
    if (activationState.templateId !== templateId) {
      return false
    }

    if (!action) {
      return true
    }

    return activationState.action === action
  }

  async function handleActivation(
    row: MessageTemplateBasicResponse,
    action: MessageTemplateActivationAction,
  ) {
    activationState.templateId = row.id
    activationState.action = action

    try {
      if (action === 'enable') {
        await enableMessageTemplate(row.id)
        ElMessage.success('启用消息模板成功')
      } else {
        await disableMessageTemplate(row.id)
        ElMessage.success('禁用消息模板成功')
      }

      await options.refreshTemplates()
    } catch (error) {
      ElMessage.error(
        extractErrorMessage(
          error,
          action === 'enable' ? '启用消息模板失败' : '禁用消息模板失败',
        ),
      )
    } finally {
      activationState.templateId = null
      activationState.action = null
    }
  }

  async function handleEnable(row: MessageTemplateBasicResponse) {
    await handleActivation(row, 'enable')
  }

  async function handleDisable(row: MessageTemplateBasicResponse) {
    await handleActivation(row, 'disable')
  }

  return {
    canDisableRow,
    canEnableRow,
    handleDisable,
    handleEnable,
    isRowActivationPending,
  }
}
