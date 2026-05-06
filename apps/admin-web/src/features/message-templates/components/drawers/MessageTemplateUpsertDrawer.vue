<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'

import { ElMessage } from 'element-plus'
import { computed, nextTick, reactive, ref, watch } from 'vue'

import {
  createMessageTemplate,
  getMessageTemplateById,
  updateMessageTemplate,
} from '@/features/message-templates/api/messageTemplatesApi'
import {
  buildCreateMessageTemplatePayload,
  buildUpdateMessageTemplatePayload,
  createDefaultMessageTemplateForm,
  createMessageTemplateFormFromDetail,
  messageTemplateFormRules,
  validateMessageTemplateContent,
} from '@/features/message-templates/models/messageTemplateForm'
import { resolveNotificationChannelTypeLabel } from '@/features/message-templates/models/messageTemplatesList'
import type {
  MessageTemplateBasicResponse,
  MessageTemplateFormModel,
  NotificationChannelType,
} from '@/features/message-templates/types/messageTemplates'
import { extractErrorMessage } from '@/shared/api/error'

const props = defineProps<{
  mode: 'create' | 'edit'
  modelValue: boolean
  template: MessageTemplateBasicResponse | null
}>()

const emit = defineEmits<{
  changed: []
  'update:modelValue': [value: boolean]
}>()

const formRef = ref<FormInstance>()
const isInitializing = ref(false)
const isSubmitting = ref(false)
const formModel = reactive<MessageTemplateFormModel>(createDefaultMessageTemplateForm())
const channelOptions: Array<{ label: string; value: NotificationChannelType }> = [
  { label: resolveNotificationChannelTypeLabel(1), value: 1 },
  { label: resolveNotificationChannelTypeLabel(2), value: 2 },
  { label: resolveNotificationChannelTypeLabel(3), value: 3 },
]

const title = computed(() => (props.mode === 'edit' ? '编辑模板' : '新建模板'))

const rules: FormRules<MessageTemplateFormModel> = {
  ...messageTemplateFormRules,
}

async function syncFormState() {
  if (!props.modelValue) {
    return
  }

  Object.assign(formModel, createDefaultMessageTemplateForm())
  await nextTick()
  formRef.value?.clearValidate()

  if (props.mode !== 'edit' || !props.template) {
    return
  }

  isInitializing.value = true

  try {
    const detail = await getMessageTemplateById(props.template.id)
    Object.assign(formModel, createMessageTemplateFormFromDetail(detail))
    await nextTick()
    formRef.value?.clearValidate()
  } catch (error) {
    ElMessage.error(extractErrorMessage(error, '加载模板详情失败'))
    closeDrawer()
  } finally {
    isInitializing.value = false
  }
}

watch(
  () => [props.modelValue, props.mode, props.template?.id] as const,
  () => {
    void syncFormState()
  },
  { immediate: true },
)

function closeDrawer() {
  emit('update:modelValue', false)
}

async function handleSubmit() {
  const form = formRef.value

  if (!form || isInitializing.value) {
    return
  }

  const isValid = await form.validate().catch(() => false)
  if (!isValid) {
    return
  }

  const contentError = validateMessageTemplateContent(formModel)
  if (contentError) {
    ElMessage.error(contentError)
    return
  }

  isSubmitting.value = true

  try {
    if (props.mode === 'edit' && props.template) {
      await updateMessageTemplate(props.template.id, buildUpdateMessageTemplatePayload(formModel))
      ElMessage.success('编辑模板成功')
    } else {
      await createMessageTemplate(buildCreateMessageTemplatePayload(formModel))
      ElMessage.success('新建模板成功')
    }

    closeDrawer()
    emit('changed')
  } catch (error) {
    ElMessage.error(extractErrorMessage(error, props.mode === 'edit' ? '编辑模板失败' : '新建模板失败'))
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <el-drawer
    :model-value="modelValue"
    :title="title"
    size="760px"
    destroy-on-close
    class="message-template-upsert-drawer"
    @close="closeDrawer"
  >
    <div v-loading="isInitializing" class="message-template-upsert-drawer__panel">
      <el-form
        ref="formRef"
        :model="formModel"
        :rules="rules"
        label-position="top"
        class="message-template-upsert-drawer__form"
      >
        <el-form-item label="模板编码" prop="code">
          <el-input
            v-model="formModel.code"
            placeholder="请输入模板编码，例如 customer_auth.phone_login_code.sms"
          />
        </el-form-item>

        <el-form-item label="模板名称" prop="name">
          <el-input v-model="formModel.name" placeholder="请输入模板名称" />
        </el-form-item>

        <el-form-item label="模板描述" prop="description">
          <el-input
            v-model="formModel.description"
            type="textarea"
            :autosize="{ minRows: 3, maxRows: 5 }"
            placeholder="请输入模板描述，可选"
          />
        </el-form-item>

        <el-form-item label="是否启用">
          <el-switch v-model="formModel.isEnabled" />
        </el-form-item>

        <el-form-item label="渠道">
          <el-select
            v-model="formModel.channelType"
            class="message-template-upsert-drawer__channel-select"
          >
            <el-option
              v-for="option in channelOptions"
              :key="option.value"
              :label="option.label"
              :value="option.value"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="标题模板">
          <el-input
            v-model="formModel.titleTemplate"
            placeholder="请输入标题模板，可选"
          />
        </el-form-item>

        <el-form-item label="正文模板">
          <el-input
            v-model="formModel.bodyTemplate"
            type="textarea"
            :autosize="{ minRows: 6, maxRows: 10 }"
            placeholder="请输入正文模板"
          />
        </el-form-item>
      </el-form>
    </div>

    <template #footer>
      <div class="message-template-upsert-drawer__footer">
        <el-button @click="closeDrawer">取消</el-button>
        <el-button type="primary" :loading="isSubmitting" @click="handleSubmit">
          {{ mode === 'edit' ? '保存修改' : '创建模板' }}
        </el-button>
      </div>
    </template>
  </el-drawer>
</template>

<style scoped>
.message-template-upsert-drawer :deep(.el-drawer) {
  background: linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.message-template-upsert-drawer :deep(.el-drawer__header) {
  margin-bottom: 0;
  padding: 22px 24px 18px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.92);
}

.message-template-upsert-drawer :deep(.el-drawer__title) {
  color: #0f172a;
  font-size: 18px;
  font-weight: 700;
  letter-spacing: -0.02em;
}

.message-template-upsert-drawer :deep(.el-drawer__body) {
  padding: 22px 24px 18px;
}

.message-template-upsert-drawer :deep(.el-drawer__footer) {
  padding: 16px 24px 22px;
  border-top: 1px solid rgba(226, 232, 240, 0.92);
  background: rgba(248, 250, 252, 0.88);
}

.message-template-upsert-drawer__panel {
  display: grid;
  gap: 18px;
}

.message-template-upsert-drawer__form {
  padding-right: 6px;
}

.message-template-upsert-drawer__channel-select {
  width: 100%;
}

.message-template-upsert-drawer__form :deep(.el-form-item) {
  margin-bottom: 18px;
}

.message-template-upsert-drawer__form :deep(.el-form-item:last-child) {
  margin-bottom: 0;
}

.message-template-upsert-drawer__form :deep(.el-form-item__label) {
  padding-bottom: 8px;
  color: #334155;
  font-size: 13px;
  font-weight: 600;
  line-height: 1.4;
}

.message-template-upsert-drawer__form :deep(.el-input__wrapper),
.message-template-upsert-drawer__form :deep(.el-select__wrapper) {
  min-height: 44px;
  border-radius: 14px;
  background: rgba(248, 250, 252, 0.92);
  box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.2);
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.message-template-upsert-drawer__form :deep(.el-input__wrapper:hover),
.message-template-upsert-drawer__form :deep(.el-select__wrapper:hover) {
  background: #ffffff;
  box-shadow: inset 0 0 0 1px rgba(100, 116, 139, 0.34);
}

.message-template-upsert-drawer__form :deep(.el-input__wrapper.is-focus),
.message-template-upsert-drawer__form :deep(.el-select__wrapper.is-focused) {
  background: #ffffff;
  box-shadow:
    inset 0 0 0 1px rgba(59, 130, 246, 0.64),
    0 0 0 4px rgba(59, 130, 246, 0.12);
}

.message-template-upsert-drawer__form :deep(.el-input__inner::placeholder),
.message-template-upsert-drawer__form :deep(.el-textarea__inner::placeholder),
.message-template-upsert-drawer__form :deep(.el-select__placeholder) {
  color: #94a3b8;
}

.message-template-upsert-drawer__form :deep(.el-input__inner),
.message-template-upsert-drawer__form :deep(.el-select__selected-item) {
  color: #0f172a;
}

.message-template-upsert-drawer__form :deep(.el-textarea__inner) {
  border-radius: 14px;
  background: rgba(248, 250, 252, 0.92);
  box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.2);
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.message-template-upsert-drawer__form :deep(.el-textarea__inner:hover) {
  background: #ffffff;
  box-shadow: inset 0 0 0 1px rgba(100, 116, 139, 0.34);
}

.message-template-upsert-drawer__form :deep(.el-textarea__inner:focus) {
  background: #ffffff;
  box-shadow:
    inset 0 0 0 1px rgba(59, 130, 246, 0.64),
    0 0 0 4px rgba(59, 130, 246, 0.12);
}

.message-template-upsert-drawer__form :deep(.el-switch) {
  --el-switch-on-color: #2563eb;
}

.message-template-upsert-drawer__footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.message-template-upsert-drawer__footer :deep(.el-button) {
  min-width: 104px;
  min-height: 42px;
  border-radius: 14px;
  font-weight: 600;
}

@media (max-width: 640px) {
  .message-template-upsert-drawer :deep(.el-drawer__header) {
    padding: 18px 16px 14px;
  }

  .message-template-upsert-drawer :deep(.el-drawer__body) {
    padding: 18px 16px 14px;
  }

  .message-template-upsert-drawer :deep(.el-drawer__footer) {
    padding: 14px 16px 18px;
  }

  .message-template-upsert-drawer__footer {
    flex-wrap: wrap;
  }

  .message-template-upsert-drawer__footer :deep(.el-button) {
    flex: 1 1 140px;
  }
}
</style>
