<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'

import { computed, nextTick, reactive, ref, watch } from 'vue'

import {
  buildCurrentAdminProfilePayload,
  createCurrentAdminProfileFormFromResponse,
  createDefaultCurrentAdminProfileForm,
  currentAdminProfileRules,
} from '@/features/current-admin/models/currentAdminProfileForm'
import type {
  CurrentAdminProfileFormModel,
  CurrentAdminResponse,
  UpdateCurrentAdminProfileRequest,
} from '@/features/current-admin/types/currentAdmin'

const props = defineProps<{
  isSubmitting: boolean
  modelValue: boolean
  profile: CurrentAdminResponse | null
}>()

const emit = defineEmits<{
  submit: [payload: UpdateCurrentAdminProfileRequest]
  'update:modelValue': [value: boolean]
}>()

const formRef = ref<FormInstance>()
const formModel = reactive<CurrentAdminProfileFormModel>(createDefaultCurrentAdminProfileForm())

const rules: FormRules<CurrentAdminProfileFormModel> = {
  ...currentAdminProfileRules,
}

const snapshot = computed<UpdateCurrentAdminProfileRequest | null>(() => {
  if (!props.profile) {
    return null
  }

  return buildCurrentAdminProfilePayload(createCurrentAdminProfileFormFromResponse(props.profile))
})

const draftPayload = computed<UpdateCurrentAdminProfileRequest>(() =>
  buildCurrentAdminProfilePayload(formModel),
)

const isDirty = computed(() => {
  const currentSnapshot = snapshot.value

  if (!currentSnapshot) {
    return false
  }

  return (
    currentSnapshot.userName !== draftPayload.value.userName
    || currentSnapshot.email !== draftPayload.value.email
    || currentSnapshot.displayName !== draftPayload.value.displayName
  )
})

function closeDrawer() {
  emit('update:modelValue', false)
}

function resetForm() {
  if (props.profile) {
    Object.assign(formModel, createCurrentAdminProfileFormFromResponse(props.profile))
  } else {
    Object.assign(formModel, createDefaultCurrentAdminProfileForm())
  }

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

  if (!form || !props.profile) {
    return
  }

  const isValid = await form.validate().catch(() => false)
  if (!isValid) {
    return
  }

  emit('submit', draftPayload.value)
}

watch(
  () => [props.modelValue, props.profile] as const,
  () => {
    void syncFormState()
  },
  { immediate: true },
)
</script>

<template>
  <el-drawer
    :model-value="modelValue"
    title="修改个人资料"
    size="560px"
    destroy-on-close
    class="current-admin-profile-edit-drawer"
    @close="closeDrawer"
  >
    <div class="current-admin-profile-edit-drawer__panel">
      <div class="current-admin-profile-edit-drawer__context">
        <p class="current-admin-profile-edit-drawer__label">当前账号</p>
        <p class="current-admin-profile-edit-drawer__value">
          {{ profile?.userName || '-' }}
        </p>
        <p class="current-admin-profile-edit-drawer__hint">
          手机号由上游身份体系维护，当前页面只允许更新账号、邮箱和显示名称。
        </p>
      </div>

      <el-form
        ref="formRef"
        :model="formModel"
        :rules="rules"
        label-position="top"
        class="current-admin-profile-edit-drawer__form"
      >
        <el-form-item label="账号" prop="userName">
          <el-input
            v-model="formModel.userName"
            autocomplete="username"
            placeholder="请输入账号"
          />
        </el-form-item>

        <el-form-item label="邮箱" prop="email">
          <el-input
            v-model="formModel.email"
            autocomplete="email"
            placeholder="请输入邮箱"
          />
        </el-form-item>

        <el-form-item label="显示名称" prop="displayName">
          <el-input
            v-model="formModel.displayName"
            autocomplete="name"
            placeholder="请输入显示名称"
          />
        </el-form-item>

        <el-form-item label="手机号">
          <el-input :model-value="profile?.phoneNumber || '未填写'" disabled />
        </el-form-item>
      </el-form>
    </div>

    <template #footer>
      <div class="current-admin-profile-edit-drawer__footer">
        <el-button :disabled="!isDirty || isSubmitting" @click="resetForm">恢复原值</el-button>
        <div class="current-admin-profile-edit-drawer__footer-actions">
          <el-button @click="closeDrawer">取消</el-button>
          <el-button
            type="primary"
            :disabled="!isDirty"
            :loading="isSubmitting"
            @click="handleSubmit"
          >
            保存资料
          </el-button>
        </div>
      </div>
    </template>
  </el-drawer>
</template>

<style scoped>
.current-admin-profile-edit-drawer :deep(.el-drawer) {
  display: flex;
  flex-direction: column;
  background: linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.current-admin-profile-edit-drawer :deep(.el-drawer__header) {
  margin-bottom: 0;
  padding: 18px 20px 14px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.92);
}

.current-admin-profile-edit-drawer :deep(.el-drawer__title) {
  color: #0f172a;
  font-size: 18px;
  font-weight: 700;
  letter-spacing: -0.02em;
}

.current-admin-profile-edit-drawer :deep(.el-drawer__body) {
  flex: 1;
  min-height: 0;
  padding: 18px 20px 14px;
  overflow: hidden;
}

.current-admin-profile-edit-drawer :deep(.el-drawer__footer) {
  padding: 14px 20px 18px;
  border-top: 1px solid rgba(226, 232, 240, 0.92);
  background: rgba(248, 250, 252, 0.88);
}

.current-admin-profile-edit-drawer__panel {
  display: grid;
  gap: 14px;
  height: 100%;
}

.current-admin-profile-edit-drawer__context {
  padding: 14px 16px;
  border: 1px solid rgba(191, 219, 254, 0.45);
  border-radius: 18px;
  background:
    linear-gradient(135deg, rgba(239, 246, 255, 0.92), rgba(255, 255, 255, 0.96));
  box-shadow: 0 10px 24px rgba(59, 130, 246, 0.08);
}

.current-admin-profile-edit-drawer__label {
  margin: 0 0 8px;
  color: #64748b;
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.current-admin-profile-edit-drawer__value {
  margin: 0;
  color: #0f172a;
  font-size: 18px;
  font-weight: 700;
  line-height: 1.4;
}

.current-admin-profile-edit-drawer__hint {
  margin: 6px 0 0;
  color: #64748b;
  font-size: 12px;
  line-height: 1.6;
}

.current-admin-profile-edit-drawer__form :deep(.el-form-item) {
  margin-bottom: 14px;
}

.current-admin-profile-edit-drawer__form :deep(.el-form-item:last-child) {
  margin-bottom: 0;
}

.current-admin-profile-edit-drawer__form :deep(.el-form-item__label) {
  padding-bottom: 8px;
  color: #334155;
  font-size: 13px;
  font-weight: 600;
  line-height: 1.4;
}

.current-admin-profile-edit-drawer__form :deep(.el-input__wrapper) {
  min-height: 44px;
  border-radius: 14px;
  background: rgba(248, 250, 252, 0.92);
  box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.2);
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.current-admin-profile-edit-drawer__form :deep(.el-input__wrapper:hover) {
  background: #ffffff;
  box-shadow: inset 0 0 0 1px rgba(100, 116, 139, 0.34);
}

.current-admin-profile-edit-drawer__form :deep(.el-input__wrapper.is-focus) {
  background: #ffffff;
  box-shadow:
    inset 0 0 0 1px rgba(59, 130, 246, 0.64),
    0 0 0 4px rgba(59, 130, 246, 0.12);
}

.current-admin-profile-edit-drawer__form :deep(.el-input.is-disabled .el-input__wrapper) {
  color: #475569;
  background: rgba(241, 245, 249, 0.96);
  box-shadow: inset 0 0 0 1px rgba(203, 213, 225, 0.82);
}

.current-admin-profile-edit-drawer__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.current-admin-profile-edit-drawer__footer-actions {
  display: flex;
  gap: 12px;
}

.current-admin-profile-edit-drawer__footer-actions :deep(.el-button + .el-button) {
  margin-left: 0;
}

.current-admin-profile-edit-drawer__footer :deep(.el-button) {
  min-width: 104px;
  min-height: 42px;
  border-radius: 14px;
  font-weight: 600;
}

@media (max-width: 640px) {
  .current-admin-profile-edit-drawer :deep(.el-drawer__header) {
    padding: 18px 16px 14px;
  }

  .current-admin-profile-edit-drawer :deep(.el-drawer__body) {
    padding: 18px 16px 14px;
    overflow: auto;
  }

  .current-admin-profile-edit-drawer :deep(.el-drawer__footer) {
    padding: 14px 16px 18px;
  }

  .current-admin-profile-edit-drawer__context {
    padding: 14px 16px;
    border-radius: 16px;
  }

  .current-admin-profile-edit-drawer__footer {
    flex-direction: column;
    align-items: stretch;
  }

  .current-admin-profile-edit-drawer__footer-actions {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
