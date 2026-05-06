<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'

import { ElMessage } from 'element-plus'
import { nextTick, reactive, ref, watch } from 'vue'

import { createAdminUser } from '@/features/admin-users/api/adminUsersApi'
import {
  adminUserProfileRules,
  buildCreateAdminUserPayload,
  createDefaultAdminUserCreateForm,
} from '@/features/admin-users/models/adminUserProfileForm'
import type { AdminUserCreateFormModel } from '@/features/admin-users/types/adminUsers'
import { extractErrorMessage } from '@/shared/api/error'

const props = defineProps<{
  modelValue: boolean
}>()

const emit = defineEmits<{
  created: []
  'update:modelValue': [value: boolean]
}>()

const formRef = ref<FormInstance>()
const isSubmitting = ref(false)
const formModel = reactive<AdminUserCreateFormModel>(createDefaultAdminUserCreateForm())

const rules: FormRules<AdminUserCreateFormModel> = {
  ...adminUserProfileRules,
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    {
      validator: (_rule, value: string, callback) => {
        const length = value.trim().length

        if (length === 0) {
          callback(new Error('请输入密码'))
          return
        }

        if (length < 8) {
          callback(new Error('密码长度至少 8 位'))
          return
        }

        if (length > 128) {
          callback(new Error('密码长度不能超过 128 个字符'))
          return
        }

        callback()
      },
      trigger: 'blur',
    },
  ],
}

function resetForm() {
  Object.assign(formModel, createDefaultAdminUserCreateForm())
  formRef.value?.clearValidate()
}

async function syncFormState() {
  if (!props.modelValue) {
    return
  }

  Object.assign(formModel, createDefaultAdminUserCreateForm())
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

  if (!form) {
    return
  }

  const isValid = await form.validate().catch(() => false)

  if (!isValid) {
    return
  }

  isSubmitting.value = true

  try {
    await createAdminUser(buildCreateAdminUserPayload(formModel))
    ElMessage.success('新增管理员成功')
    resetForm()
    closeDrawer()
    emit('created')
  } catch (error) {
    ElMessage.error(extractErrorMessage(error, '新增管理员失败'))
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <el-drawer
    :model-value="modelValue"
    title="新增管理员"
    size="560px"
    destroy-on-close
    class="admin-user-create-drawer"
    @close="closeDrawer"
  >
    <div class="admin-user-create-drawer__panel">
      <el-form
        ref="formRef"
        :model="formModel"
        :rules="rules"
        label-position="top"
        class="admin-user-create-drawer__form"
      >
        <el-form-item label="账号" prop="userName">
          <el-input v-model="formModel.userName" placeholder="请输入账号" />
        </el-form-item>

        <el-form-item label="邮箱" prop="email">
          <el-input v-model="formModel.email" placeholder="请输入邮箱" />
        </el-form-item>

        <el-form-item label="显示名称" prop="displayName">
          <el-input v-model="formModel.displayName" placeholder="请输入显示名称" />
        </el-form-item>

        <el-form-item label="手机号" prop="phoneNumber">
          <el-input v-model="formModel.phoneNumber" placeholder="请输入手机号，可选" />
        </el-form-item>

        <el-form-item label="密码" prop="password">
          <el-input
            v-model="formModel.password"
            type="password"
            show-password
            placeholder="请输入密码"
          />
        </el-form-item>

        <el-form-item label="是否启用" prop="isActive">
          <el-switch v-model="formModel.isActive" />
        </el-form-item>
      </el-form>
    </div>

    <template #footer>
      <div class="admin-user-create-drawer__footer">
        <el-button @click="closeDrawer">取消</el-button>
        <el-button type="primary" :loading="isSubmitting" @click="handleSubmit">
          创建管理员
        </el-button>
      </div>
    </template>
  </el-drawer>
</template>

<style scoped>
.admin-user-create-drawer :deep(.el-drawer) {
  background: linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.admin-user-create-drawer :deep(.el-drawer__header) {
  margin-bottom: 0;
  padding: 22px 24px 18px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.92);
}

.admin-user-create-drawer :deep(.el-drawer__title) {
  color: #0f172a;
  font-size: 18px;
  font-weight: 700;
  letter-spacing: -0.02em;
}

.admin-user-create-drawer :deep(.el-drawer__body) {
  padding: 22px 24px 18px;
}

.admin-user-create-drawer :deep(.el-drawer__footer) {
  padding: 16px 24px 22px;
  border-top: 1px solid rgba(226, 232, 240, 0.92);
  background: rgba(248, 250, 252, 0.88);
}

.admin-user-create-drawer__panel {
  display: grid;
  gap: 18px;
}

.admin-user-create-drawer__form {
  padding-right: 6px;
}

.admin-user-create-drawer__form :deep(.el-form-item) {
  margin-bottom: 18px;
}

.admin-user-create-drawer__form :deep(.el-form-item:last-child) {
  margin-bottom: 0;
}

.admin-user-create-drawer__form :deep(.el-form-item__label) {
  padding-bottom: 8px;
  color: #334155;
  font-size: 13px;
  font-weight: 600;
  line-height: 1.4;
}

.admin-user-create-drawer__form :deep(.el-input__wrapper) {
  min-height: 44px;
  border-radius: 14px;
  background: rgba(248, 250, 252, 0.92);
  box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.2);
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.admin-user-create-drawer__form :deep(.el-input__wrapper:hover) {
  background: #ffffff;
  box-shadow: inset 0 0 0 1px rgba(100, 116, 139, 0.34);
}

.admin-user-create-drawer__form :deep(.el-input__wrapper.is-focus) {
  background: #ffffff;
  box-shadow:
    inset 0 0 0 1px rgba(59, 130, 246, 0.64),
    0 0 0 4px rgba(59, 130, 246, 0.12);
}

.admin-user-create-drawer__form :deep(.el-input__inner::placeholder) {
  color: #94a3b8;
}

.admin-user-create-drawer__form :deep(.el-switch) {
  --el-switch-on-color: #2563eb;
}

.admin-user-create-drawer__footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.admin-user-create-drawer__footer :deep(.el-button) {
  min-width: 104px;
  min-height: 42px;
  border-radius: 14px;
  font-weight: 600;
}

@media (max-width: 640px) {
  .admin-user-create-drawer :deep(.el-drawer__header) {
    padding: 18px 16px 14px;
  }

  .admin-user-create-drawer :deep(.el-drawer__body) {
    padding: 18px 16px 14px;
  }

  .admin-user-create-drawer :deep(.el-drawer__footer) {
    padding: 14px 16px 18px;
  }

  .admin-user-create-drawer__footer {
    flex-wrap: wrap;
  }

  .admin-user-create-drawer__footer :deep(.el-button) {
    flex: 1 1 140px;
  }
}
</style>
