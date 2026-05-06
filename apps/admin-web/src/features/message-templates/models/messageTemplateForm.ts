import type { FormItemRule } from 'element-plus'

import type {
  CreateMessageTemplateRequest,
  MessageTemplateDetailResponse,
  MessageTemplateFormModel,
  UpdateMessageTemplateRequest,
} from '@/features/message-templates/types/messageTemplates'

export const MESSAGE_TEMPLATE_CODE_PATTERN = /^[a-z0-9_.-]+$/

export function createDefaultMessageTemplateForm(): MessageTemplateFormModel {
  return {
    code: '',
    name: '',
    description: '',
    isEnabled: true,
    channelType: 1,
    titleTemplate: '',
    bodyTemplate: '',
  }
}

export function createMessageTemplateFormFromDetail(
  detail: MessageTemplateDetailResponse,
): MessageTemplateFormModel {
  return {
    code: detail.code,
    name: detail.name,
    description: detail.description ?? '',
    isEnabled: detail.isEnabled,
    channelType: detail.channelType,
    titleTemplate: detail.titleTemplate ?? '',
    bodyTemplate: detail.bodyTemplate,
  }
}

function buildMessageTemplatePayloadBase(form: MessageTemplateFormModel) {
  const description = form.description.trim()
  const titleTemplate = form.titleTemplate.trim()

  return {
    code: form.code.trim(),
    name: form.name.trim(),
    description: description ? description : null,
    isEnabled: form.isEnabled,
    channelType: form.channelType,
    titleTemplate: titleTemplate ? titleTemplate : null,
    bodyTemplate: form.bodyTemplate.trim(),
  }
}

export function buildCreateMessageTemplatePayload(
  form: MessageTemplateFormModel,
): CreateMessageTemplateRequest {
  return buildMessageTemplatePayloadBase(form)
}

export function buildUpdateMessageTemplatePayload(
  form: MessageTemplateFormModel,
): UpdateMessageTemplateRequest {
  return buildMessageTemplatePayloadBase(form)
}

export function validateMessageTemplateContent(form: MessageTemplateFormModel) {
  if (!form.bodyTemplate.trim()) {
    return '正文模板不能为空'
  }

  if (form.bodyTemplate.trim().length > 4000) {
    return '正文模板长度不能超过 4000 个字符'
  }

  if (form.titleTemplate.trim().length > 256) {
    return '标题模板长度不能超过 256 个字符'
  }

  return null
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

function validateTemplateCode(_rule: unknown, value: string, callback: (error?: Error) => void) {
  const code = value.trim()

  if (!code) {
    callback(new Error('请输入模板编码'))
    return
  }

  if (code.length > 128) {
    callback(new Error('模板编码长度不能超过 128 个字符'))
    return
  }

  if (!MESSAGE_TEMPLATE_CODE_PATTERN.test(code)) {
    callback(new Error('模板编码仅支持小写字母、数字、点、下划线和中划线'))
    return
  }

  callback()
}

function validateDescription(_rule: unknown, value: string, callback: (error?: Error) => void) {
  if (value.trim().length > 256) {
    callback(new Error('模板描述长度不能超过 256 个字符'))
    return
  }

  callback()
}

export const messageTemplateFormRules = {
  code: [
    { required: true, message: '请输入模板编码', trigger: 'blur' },
    {
      validator: validateTemplateCode,
      trigger: 'blur',
    },
  ],
  name: [
    { required: true, message: '请输入模板名称', trigger: 'blur' },
    {
      validator: createRequiredTrimmedValidator(
        '请输入模板名称',
        128,
        '模板名称长度不能超过 128 个字符',
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
} satisfies Record<'code' | 'name' | 'description', FormItemRule[]>
