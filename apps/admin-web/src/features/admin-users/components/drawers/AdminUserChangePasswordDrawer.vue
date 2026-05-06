<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'

import { ElMessage } from 'element-plus'
import { computed, nextTick, reactive, ref, watch } from 'vue'

import { changeAdminUserPassword } from '@/features/admin-users/api/adminUsersApi'
import {
  buildChangeAdminUserPasswordPayload,
  createDefaultAdminUserChangePasswordForm,
  isAdminUserPasswordConfirmationMatched,
} from '@/features/admin-users/models/adminUserPasswordForm'
import type {
  AdminUserBasicResponse,
  AdminUserChangePasswordFormModel,
} from '@/features/admin-users/types/adminUsers'
import { extractErrorMessage } from '@/shared/api/error'

const props = defineProps<{
  modelValue: boolean
  user: AdminUserBasicResponse | null
}>()

const emit = defineEmits<{
  changed: []
  'update:modelValue': [value: boolean]
}>()

const formRef = ref<FormInstance>()
const isSubmitting = ref(false)
const formModel = reactive<AdminUserChangePasswordFormModel>(
  createDefaultAdminUserChangePasswordForm(),
)

const userDisplay = computed(() => {
  if (!props.user) {
    return '-'
  }

  return props.user.displayName
    ? `${props.user.userName} / ${props.user.displayName}`
    : props.user.userName
})

const rules: FormRules<AdminUserChangePasswordFormModel> = {
  newPassword: [
    { required: true, message: '请输入新密码', trigger: 'blur' },
    {
      validator: (_rule, value: string, callback) => {
        const length = value.trim().length

        if (length === 0) {
          callback(new Error('请输入新密码'))
          return
        }

        if (length < 8) {
          callback(new Error('新密码长度至少 8 位'))
          return
        }

        if (length > 128) {
          callback(new Error('新密码长度不能超过 128 个字符'))
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
      validator: (_rule, _value: string, callback) => {
        if (!formModel.confirmPassword.trim()) {
          callback(new Error('请再次输入新密码'))
          return
        }

        if (!isAdminUserPasswordConfirmationMatched(formModel)) {
          callback(new Error('两次输入的新密码不一致'))
          return
        }

        callback()
      },
      trigger: ['blur', 'change'],
    },
  ],
}

function resetForm() {
  Object.assign(formModel, createDefaultAdminUserChangePasswordForm())
  formRef.value?.clearValidate()
}

async function syncFormState() {
  if (!props.modelValue) {
    return
  }

  Object.assign(formModel, createDefaultAdminUserChangePasswordForm())
  await nextTick()
  formRef.value?.clearValidate()
}

watch(
  () => props.modelValue,
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

  if (!form || !props.user) {
    return
  }

  const isValid = await form.validate().catch(() => false)

  if (!isValid) {
    return
  }

  isSubmitting.value = true

  try {
    await changeAdminUserPassword(props.user.id, buildChangeAdminUserPasswordPayload(formModel))
    ElMessage.success('修改管理员密码成功')
    resetForm()
    closeDrawer()
    emit('changed')
  } catch (error) {
    ElMessage.error(extractErrorMessage(error, '修改管理员密码失败'))
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <el-drawer
    :model-value="modelValue"
    title="修改管理员密码"
    size="560px"
    destroy-on-close
    class="admin-user-change-password-drawer"
    @close="closeDrawer"
  >
    <div class="admin-user-change-password-drawer__panel">
      <div class="admin-user-change-password-drawer__context">
        <p class="admin-user-change-password-drawer__label">当前操作对象</p>
        <p class="admin-user-change-password-drawer__value">{{ userDisplay }}</p>
      </div>

      <el-form
        ref="formRef"
        :model="formModel"
        :rules="rules"
        label-position="top"
        class="admin-user-change-password-drawer__form"
      >
        <el-form-item label="新密码" prop="newPassword">
          <el-input
            v-model="formModel.newPassword"
            type="password"
            show-password
            placeholder="请输入新密码"
          />
        </el-form-item>

        <el-form-item label="确认新密码" prop="confirmPassword">
          <el-input
            v-model="formModel.confirmPassword"
            type="password"
            show-password
            placeholder="请再次输入新密码"
          />
        </el-form-item>
      </el-form>
    </div>

    <template #footer>
      <div class="admin-user-change-password-drawer__footer">
        <el-button @click="closeDrawer">取消</el-button>
        <el-button type="primary" :loading="isSubmitting" @click="handleSubmit">
          保存新密码
        </el-button>
      </div>
    </template>
  </el-drawer>
</template>

<style scoped>
.admin-user-change-password-drawer :deep(.el-drawer) {
  background: linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.admin-user-change-password-drawer :deep(.el-drawer__header) {
  margin-bottom: 0;
  padding: 22px 24px 18px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.92);
}

.admin-user-change-password-drawer :deep(.el-drawer__title) {
  color: #0f172a;
  font-size: 18px;
  font-weight: 700;
  letter-spacing: -0.02em;
}

.admin-user-change-password-drawer :deep(.el-drawer__body) {
  padding: 22px 24px 18px;
}

.admin-user-change-password-drawer :deep(.el-drawer__footer) {
  padding: 16px 24px 22px;
  border-top: 1px solid rgba(226, 232, 240, 0.92);
  background: rgba(248, 250, 252, 0.88);
}

.admin-user-change-password-drawer__panel {
  display: grid;
  gap: 18px;
}

.admin-user-change-password-drawer__context {
  padding: 16px 18px;
  border: 1px solid rgba(191, 219, 254, 0.45);
  border-radius: 18px;
  background:
    linear-gradient(135deg, rgba(239, 246, 255, 0.92), rgba(255, 255, 255, 0.96));
  box-shadow: 0 10px 24px rgba(59, 130, 246, 0.08);
}

.admin-user-change-password-drawer__label {
  margin: 0 0 8px;
  color: #64748b;
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.admin-user-change-password-drawer__value {
  margin: 0;
  color: #0f172a;
  font-weight: 700;
  line-height: 1.5;
}

.admin-user-change-password-drawer__form {
  padding-right: 6px;
}

.admin-user-change-password-drawer__form :deep(.el-form-item) {
  margin-bottom: 18px;
}

.admin-user-change-password-drawer__form :deep(.el-form-item:last-child) {
  margin-bottom: 0;
}

.admin-user-change-password-drawer__form :deep(.el-form-item__label) {
  padding-bottom: 8px;
  color: #334155;
  font-size: 13px;
  font-weight: 600;
  line-height: 1.4;
}

.admin-user-change-password-drawer__form :deep(.el-input__wrapper) {
  min-height: 44px;
  border-radius: 14px;
  background: rgba(248, 250, 252, 0.92);
  box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.2);
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.admin-user-change-password-drawer__form :deep(.el-input__wrapper:hover) {
  background: #ffffff;
  box-shadow: inset 0 0 0 1px rgba(100, 116, 139, 0.34);
}

.admin-user-change-password-drawer__form :deep(.el-input__wrapper.is-focus) {
  background: #ffffff;
  box-shadow:
    inset 0 0 0 1px rgba(59, 130, 246, 0.64),
    0 0 0 4px rgba(59, 130, 246, 0.12);
}

.admin-user-change-password-drawer__form :deep(.el-input__inner::placeholder) {
  color: #94a3b8;
}

.admin-user-change-password-drawer__footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.admin-user-change-password-drawer__footer :deep(.el-button) {
  min-width: 104px;
  min-height: 42px;
  border-radius: 14px;
  font-weight: 600;
}

@media (max-width: 640px) {
  .admin-user-change-password-drawer :deep(.el-drawer__header) {
    padding: 18px 16px 14px;
  }

  .admin-user-change-password-drawer :deep(.el-drawer__body) {
    padding: 18px 16px 14px;
  }

  .admin-user-change-password-drawer :deep(.el-drawer__footer) {
    padding: 14px 16px 18px;
  }

  .admin-user-change-password-drawer__context {
    padding: 14px 16px;
    border-radius: 16px;
  }

  .admin-user-change-password-drawer__footer {
    flex-wrap: wrap;
  }

  .admin-user-change-password-drawer__footer :deep(.el-button) {
    flex: 1 1 140px;
  }
}
</style>
