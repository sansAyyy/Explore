<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'

import { computed, nextTick, reactive, ref, watch } from 'vue'

import {
  buildCurrentAdminPasswordPayload,
  createDefaultCurrentAdminPasswordForm,
  CURRENT_ADMIN_MIN_PASSWORD_LENGTH,
  isCurrentAdminPasswordChanged,
  isCurrentAdminPasswordConfirmationMatched,
} from '@/features/current-admin/models/currentAdminPasswordForm'
import type {
  ChangeCurrentAdminPasswordRequest,
  CurrentAdminPasswordFormModel,
} from '@/features/current-admin/types/currentAdmin'

type PasswordChecklistItem = {
  label: string
  passed: boolean
}

const props = defineProps<{
  isSubmitting: boolean
  modelValue: boolean
}>()

const emit = defineEmits<{
  submit: [payload: ChangeCurrentAdminPasswordRequest]
  'update:modelValue': [value: boolean]
}>()

const formRef = ref<FormInstance>()
const formModel = reactive<CurrentAdminPasswordFormModel>(createDefaultCurrentAdminPasswordForm())

const rules: FormRules<CurrentAdminPasswordFormModel> = {
  currentPassword: [
    { required: true, message: '请输入当前密码', trigger: 'blur' },
  ],
  newPassword: [
    { required: true, message: '请输入新密码', trigger: 'blur' },
    {
      validator: (_rule, value: string, callback) => {
        const trimmedValue = value.trim()

        if (trimmedValue.length < CURRENT_ADMIN_MIN_PASSWORD_LENGTH) {
          callback(new Error(`新密码至少需要 ${CURRENT_ADMIN_MIN_PASSWORD_LENGTH} 位`))
          return
        }

        if (
          !isCurrentAdminPasswordChanged({
            currentPassword: formModel.currentPassword,
            newPassword: value,
          })
        ) {
          callback(new Error('新密码不能与当前密码相同'))
          return
        }

        callback()
      },
      trigger: 'blur',
    },
  ],
  confirmPassword: [
    { required: true, message: '请再次输入新密码', trigger: 'blur' },
    {
      validator: (_rule, value: string, callback) => {
        if (
          !isCurrentAdminPasswordConfirmationMatched({
            newPassword: formModel.newPassword,
            confirmPassword: value,
          })
        ) {
          callback(new Error('两次输入的新密码不一致'))
          return
        }

        callback()
      },
      trigger: ['blur', 'change'],
    },
  ],
}

const hasDraft = computed(() =>
  [
    formModel.currentPassword,
    formModel.newPassword,
    formModel.confirmPassword,
  ].some((value) => value.trim().length > 0),
)

const checklist = computed<PasswordChecklistItem[]>(() => {
  const password = formModel.newPassword.trim()

  return [
    {
      label: `长度不少于 ${CURRENT_ADMIN_MIN_PASSWORD_LENGTH} 位`,
      passed: password.length >= CURRENT_ADMIN_MIN_PASSWORD_LENGTH,
    },
    {
      label: '包含大写与小写字母',
      passed: /[A-Z]/.test(password) && /[a-z]/.test(password),
    },
    {
      label: '包含至少一个数字',
      passed: /\d/.test(password),
    },
    {
      label: '包含至少一个符号字符',
      passed: /[^A-Za-z0-9]/.test(password),
    },
  ]
})

const canSubmit = computed(() => {
  const currentPassword = formModel.currentPassword.trim()
  const newPassword = formModel.newPassword.trim()
  const confirmPassword = formModel.confirmPassword.trim()

  return (
    currentPassword.length > 0
    && newPassword.length >= CURRENT_ADMIN_MIN_PASSWORD_LENGTH
    && confirmPassword.length > 0
    && isCurrentAdminPasswordChanged({
      currentPassword: formModel.currentPassword,
      newPassword: formModel.newPassword,
    })
    && isCurrentAdminPasswordConfirmationMatched({
      newPassword: formModel.newPassword,
      confirmPassword: formModel.confirmPassword,
    })
  )
})

function closeDrawer() {
  emit('update:modelValue', false)
}

function resetForm() {
  Object.assign(formModel, createDefaultCurrentAdminPasswordForm())
  formRef.value?.clearValidate()
}

async function syncFormState() {
  if (!props.modelValue) {
    return
  }

  resetForm()
  await nextTick()
  formRef.value?.clearValidate()
}

async function handleSubmit() {
  const form = formRef.value
  if (!form) {
    return
  }

  const isValid = await form.validate().catch(() => false)
  if (!isValid) {
    return
  }

  emit('submit', buildCurrentAdminPasswordPayload(formModel))
}

watch(
  () => props.modelValue,
  () => {
    void syncFormState()
  },
  { immediate: true },
)

watch(
  () => formModel.newPassword,
  () => {
    if (!formModel.confirmPassword) {
      return
    }

    const validationPromise = formRef.value?.validateField('confirmPassword')
    if (!validationPromise) {
      return
    }

    void validationPromise.catch(() => undefined)
  },
)
</script>

<template>
  <el-drawer
    :model-value="modelValue"
    title="修改登录密码"
    size="560px"
    destroy-on-close
    class="current-admin-password-drawer"
    @close="closeDrawer"
  >
    <div class="current-admin-password-drawer__panel">
      <div class="current-admin-password-drawer__context">
        <p class="current-admin-password-drawer__label">安全建议</p>
        <p class="current-admin-password-drawer__value">建议使用独占密码，并定期轮换。</p>
        <ul class="current-admin-password-drawer__checklist">
          <li
            v-for="item in checklist"
            :key="item.label"
            :class="{ 'current-admin-password-drawer__checklist-item--passed': item.passed }"
          >
            <span class="current-admin-password-drawer__checkmark" aria-hidden="true" />
            <span>{{ item.label }}</span>
          </li>
        </ul>
      </div>

      <el-form
        ref="formRef"
        :model="formModel"
        :rules="rules"
        label-position="top"
        class="current-admin-password-drawer__form"
      >
        <el-form-item label="当前密码" prop="currentPassword">
          <el-input
            v-model="formModel.currentPassword"
            autocomplete="current-password"
            type="password"
            show-password
            placeholder="请输入当前密码"
          />
        </el-form-item>

        <el-form-item label="新密码" prop="newPassword">
          <el-input
            v-model="formModel.newPassword"
            autocomplete="new-password"
            type="password"
            show-password
            placeholder="请输入新密码"
          />
        </el-form-item>

        <el-form-item label="确认新密码" prop="confirmPassword">
          <el-input
            v-model="formModel.confirmPassword"
            autocomplete="new-password"
            type="password"
            show-password
            placeholder="请再次输入新密码"
          />
        </el-form-item>
      </el-form>
    </div>

    <template #footer>
      <div class="current-admin-password-drawer__footer">
        <el-button :disabled="!hasDraft || isSubmitting" @click="resetForm">清空表单</el-button>
        <div class="current-admin-password-drawer__footer-actions">
          <el-button @click="closeDrawer">取消</el-button>
          <el-button
            type="primary"
            :disabled="!canSubmit"
            :loading="isSubmitting"
            @click="handleSubmit"
          >
            更新密码
          </el-button>
        </div>
      </div>
    </template>
  </el-drawer>
</template>

<style scoped>
.current-admin-password-drawer :deep(.el-drawer) {
  display: flex;
  flex-direction: column;
  background: linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.current-admin-password-drawer :deep(.el-drawer__header) {
  margin-bottom: 0;
  padding: 18px 20px 14px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.92);
}

.current-admin-password-drawer :deep(.el-drawer__title) {
  color: #0f172a;
  font-size: 18px;
  font-weight: 700;
  letter-spacing: -0.02em;
}

.current-admin-password-drawer :deep(.el-drawer__body) {
  flex: 1;
  min-height: 0;
  padding: 18px 20px 14px;
  overflow: hidden;
}

.current-admin-password-drawer :deep(.el-drawer__footer) {
  padding: 14px 20px 18px;
  border-top: 1px solid rgba(226, 232, 240, 0.92);
  background: rgba(248, 250, 252, 0.88);
}

.current-admin-password-drawer__panel {
  display: grid;
  gap: 14px;
  height: 100%;
}

.current-admin-password-drawer__context {
  padding: 14px 16px;
  border: 1px solid rgba(191, 219, 254, 0.45);
  border-radius: 18px;
  background:
    linear-gradient(135deg, rgba(239, 246, 255, 0.92), rgba(255, 255, 255, 0.96));
  box-shadow: 0 10px 24px rgba(59, 130, 246, 0.08);
}

.current-admin-password-drawer__label {
  margin: 0 0 8px;
  color: #64748b;
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.current-admin-password-drawer__value {
  margin: 0;
  color: #0f172a;
  font-size: 15px;
  font-weight: 700;
  line-height: 1.5;
}

.current-admin-password-drawer__checklist {
  display: grid;
  gap: 8px;
  margin: 12px 0 0;
  padding: 0;
  list-style: none;
}

.current-admin-password-drawer__checklist li {
  display: flex;
  align-items: center;
  gap: 10px;
  color: #64748b;
  font-size: 12px;
  line-height: 1.5;
}

.current-admin-password-drawer__checklist-item--passed {
  color: #0f172a;
}

.current-admin-password-drawer__checkmark {
  flex: none;
  width: 10px;
  height: 10px;
  border-radius: 999px;
  background: rgba(148, 163, 184, 0.6);
  box-shadow: 0 0 0 5px rgba(226, 232, 240, 0.56);
}

.current-admin-password-drawer__checklist-item--passed .current-admin-password-drawer__checkmark {
  background: #22c55e;
  box-shadow: 0 0 0 5px rgba(187, 247, 208, 0.52);
}

.current-admin-password-drawer__form :deep(.el-form-item) {
  margin-bottom: 14px;
}

.current-admin-password-drawer__form :deep(.el-form-item:last-child) {
  margin-bottom: 0;
}

.current-admin-password-drawer__form :deep(.el-form-item__label) {
  padding-bottom: 8px;
  color: #334155;
  font-size: 13px;
  font-weight: 600;
  line-height: 1.4;
}

.current-admin-password-drawer__form :deep(.el-input__wrapper) {
  min-height: 44px;
  border-radius: 14px;
  background: rgba(248, 250, 252, 0.92);
  box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.2);
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.current-admin-password-drawer__form :deep(.el-input__wrapper:hover) {
  background: #ffffff;
  box-shadow: inset 0 0 0 1px rgba(100, 116, 139, 0.34);
}

.current-admin-password-drawer__form :deep(.el-input__wrapper.is-focus) {
  background: #ffffff;
  box-shadow:
    inset 0 0 0 1px rgba(59, 130, 246, 0.64),
    0 0 0 4px rgba(59, 130, 246, 0.12);
}

.current-admin-password-drawer__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.current-admin-password-drawer__footer-actions {
  display: flex;
  gap: 12px;
}

.current-admin-password-drawer__footer-actions :deep(.el-button + .el-button) {
  margin-left: 0;
}

.current-admin-password-drawer__footer :deep(.el-button) {
  min-width: 104px;
  min-height: 42px;
  border-radius: 14px;
  font-weight: 600;
}

@media (max-width: 640px) {
  .current-admin-password-drawer :deep(.el-drawer__header) {
    padding: 18px 16px 14px;
  }

  .current-admin-password-drawer :deep(.el-drawer__body) {
    padding: 18px 16px 14px;
    overflow: auto;
  }

  .current-admin-password-drawer :deep(.el-drawer__footer) {
    padding: 14px 16px 18px;
  }

  .current-admin-password-drawer__context {
    padding: 14px 16px;
    border-radius: 16px;
  }

  .current-admin-password-drawer__footer {
    flex-direction: column;
    align-items: stretch;
  }

  .current-admin-password-drawer__footer-actions {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
