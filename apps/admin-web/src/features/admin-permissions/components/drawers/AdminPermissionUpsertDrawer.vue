<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'

import { ElMessage } from 'element-plus'
import { computed, nextTick, reactive, ref, watch } from 'vue'

import {
  createAdminPermission,
  updateAdminPermission,
} from '@/features/admin-permissions/api/adminPermissionsApi'
import {
  buildCreateAdminPermissionPayload,
  buildUpdateAdminPermissionPayload,
  createAdminPermissionFormFromDetail,
  createDefaultAdminPermissionForm,
} from '@/features/admin-permissions/models/adminPermissionForm'
import type {
  AdminPermissionFormModel,
  AdminPermissionTreeNode,
} from '@/features/admin-permissions/types/adminPermissions'
import { extractErrorMessage } from '@/shared/api/error'

const props = defineProps<{
  initialParentId?: string | null
  mode: 'create' | 'edit'
  modelValue: boolean
  parentOptions: AdminPermissionTreeNode[]
  permission: AdminPermissionTreeNode | null
}>()

const emit = defineEmits<{
  changed: []
  'update:modelValue': [value: boolean]
}>()

const formRef = ref<FormInstance>()
const isSubmitting = ref(false)
const formModel = reactive<AdminPermissionFormModel>(createDefaultAdminPermissionForm())

const title = computed(() => {
  if (props.mode === 'edit') {
    return '编辑权限'
  }

  return props.initialParentId ? '新增子权限' : '新增根权限'
})

const rules: FormRules<AdminPermissionFormModel> = {
  parentId: [],
  code: [
    { required: true, message: '请输入权限编码', trigger: 'blur' },
    {
      validator: (_rule, value: string, callback) => {
        const code = value.trim()

        if (!code) {
          callback(new Error('请输入权限编码'))
          return
        }

        if (code.length > 128) {
          callback(new Error('权限编码长度不能超过 128 个字符'))
          return
        }

        if (!/^[a-z0-9_.]+$/.test(code)) {
          callback(new Error('权限编码仅支持小写字母、数字、下划线和点'))
          return
        }

        callback()
      },
      trigger: 'blur',
    },
  ],
  name: [
    { required: true, message: '请输入权限名称', trigger: 'blur' },
    {
      validator: (_rule, value: string, callback) => {
        const name = value.trim()

        if (!name) {
          callback(new Error('请输入权限名称'))
          return
        }

        if (name.length > 128) {
          callback(new Error('权限名称长度不能超过 128 个字符'))
          return
        }

        callback()
      },
      trigger: 'blur',
    },
  ],
  description: [
    {
      validator: (_rule, value: string, callback) => {
        if (value.trim().length > 256) {
          callback(new Error('权限描述长度不能超过 256 个字符'))
          return
        }

        callback()
      },
      trigger: 'blur',
    },
  ],
  resourceType: [{ required: true, message: '请选择权限类型', trigger: 'change' }],
  isActive: [],
}

async function syncFormState() {
  if (!props.modelValue) {
    return
  }

  Object.assign(formModel, createDefaultAdminPermissionForm())

  if (props.mode === 'edit' && props.permission) {
    Object.assign(
      formModel,
      createAdminPermissionFormFromDetail({
        parentId: props.permission.parentId,
        code: props.permission.code,
        name: props.permission.name,
        description: props.permission.description,
        resourceType: props.permission.resourceType,
        isActive: props.permission.isActive,
      }),
    )
  } else {
    formModel.parentId = props.initialParentId ?? null
  }

  await nextTick()
  formRef.value?.clearValidate()
}

watch(
  () => [props.modelValue, props.mode, props.permission?.id, props.initialParentId] as const,
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
    if (props.mode === 'edit' && props.permission) {
      await updateAdminPermission(props.permission.id, buildUpdateAdminPermissionPayload(formModel))
      ElMessage.success('编辑权限成功')
    } else {
      await createAdminPermission(buildCreateAdminPermissionPayload(formModel))
      ElMessage.success('新增权限成功')
    }

    closeDrawer()
    emit('changed')
  } catch (error) {
    ElMessage.error(
      extractErrorMessage(error, props.mode === 'edit' ? '编辑权限失败' : '新增权限失败'),
    )
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <el-drawer
    :model-value="modelValue"
    :title="title"
    size="620px"
    destroy-on-close
    class="admin-permission-upsert-drawer"
    @close="closeDrawer"
  >
    <div class="admin-permission-upsert-drawer__panel">
      <el-form
        ref="formRef"
        :model="formModel"
        :rules="rules"
        label-position="top"
        class="admin-permission-upsert-drawer__form"
      >
        <el-form-item label="父级权限" prop="parentId">
          <el-tree-select
            v-model="formModel.parentId"
            :data="parentOptions"
            clearable
            check-strictly
            default-expand-all
            node-key="id"
            placeholder="不选择则创建为根权限"
            :props="{
              label: 'name',
              children: 'children',
              disabled: 'disabled',
            }"
          />
        </el-form-item>

        <el-form-item label="权限编码" prop="code">
          <el-input v-model="formModel.code" placeholder="请输入权限编码" />
        </el-form-item>

        <el-form-item label="权限名称" prop="name">
          <el-input v-model="formModel.name" placeholder="请输入权限名称" />
        </el-form-item>

        <el-form-item label="权限描述" prop="description">
          <el-input
            v-model="formModel.description"
            type="textarea"
            :autosize="{ minRows: 3, maxRows: 5 }"
            placeholder="请输入权限描述，可选"
          />
        </el-form-item>

        <el-form-item label="权限类型" prop="resourceType">
          <el-radio-group v-model="formModel.resourceType" class="admin-permission-upsert-drawer__radio-group">
            <el-radio :value="1">页面权限</el-radio>
            <el-radio :value="2">按钮权限</el-radio>
            <el-radio :value="3">分组节点</el-radio>
          </el-radio-group>
        </el-form-item>

        <el-form-item v-if="mode === 'create'" label="是否启用" prop="isActive">
          <el-switch v-model="formModel.isActive" />
        </el-form-item>
      </el-form>
    </div>

    <template #footer>
      <div class="admin-permission-upsert-drawer__footer">
        <el-button @click="closeDrawer">取消</el-button>
        <el-button type="primary" :loading="isSubmitting" @click="handleSubmit">
          {{ mode === 'edit' ? '保存修改' : '创建权限' }}
        </el-button>
      </div>
    </template>
  </el-drawer>
</template>

<style scoped>
.admin-permission-upsert-drawer :deep(.el-drawer) {
  background: linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.admin-permission-upsert-drawer :deep(.el-drawer__header) {
  margin-bottom: 0;
  padding: 22px 24px 18px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.92);
}

.admin-permission-upsert-drawer :deep(.el-drawer__title) {
  color: #0f172a;
  font-size: 18px;
  font-weight: 700;
  letter-spacing: -0.02em;
}

.admin-permission-upsert-drawer :deep(.el-drawer__body) {
  padding: 22px 24px 18px;
}

.admin-permission-upsert-drawer :deep(.el-drawer__footer) {
  padding: 16px 24px 22px;
  border-top: 1px solid rgba(226, 232, 240, 0.92);
  background: rgba(248, 250, 252, 0.88);
}

.admin-permission-upsert-drawer__panel {
  display: grid;
  gap: 18px;
}

.admin-permission-upsert-drawer__form {
  padding-right: 6px;
}

.admin-permission-upsert-drawer__form :deep(.el-form-item) {
  margin-bottom: 18px;
}

.admin-permission-upsert-drawer__form :deep(.el-form-item:last-child) {
  margin-bottom: 0;
}

.admin-permission-upsert-drawer__form :deep(.el-form-item__label) {
  padding-bottom: 8px;
  color: #334155;
  font-size: 13px;
  font-weight: 600;
  line-height: 1.4;
}

.admin-permission-upsert-drawer__form :deep(.el-input__wrapper),
.admin-permission-upsert-drawer__form :deep(.el-select__wrapper),
.admin-permission-upsert-drawer__form :deep(.el-tree-select .el-select__wrapper) {
  min-height: 44px;
  border-radius: 14px;
  background: rgba(248, 250, 252, 0.92);
  box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.2);
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.admin-permission-upsert-drawer__form :deep(.el-input__wrapper:hover),
.admin-permission-upsert-drawer__form :deep(.el-select__wrapper:hover),
.admin-permission-upsert-drawer__form :deep(.el-tree-select .el-select__wrapper:hover) {
  background: #ffffff;
  box-shadow: inset 0 0 0 1px rgba(100, 116, 139, 0.34);
}

.admin-permission-upsert-drawer__form :deep(.el-input__wrapper.is-focus),
.admin-permission-upsert-drawer__form :deep(.el-select__wrapper.is-focused),
.admin-permission-upsert-drawer__form :deep(.el-tree-select .el-select__wrapper.is-focused) {
  background: #ffffff;
  box-shadow:
    inset 0 0 0 1px rgba(59, 130, 246, 0.64),
    0 0 0 4px rgba(59, 130, 246, 0.12);
}

.admin-permission-upsert-drawer__form :deep(.el-input__inner::placeholder),
.admin-permission-upsert-drawer__form :deep(.el-textarea__inner::placeholder),
.admin-permission-upsert-drawer__form :deep(.el-select__placeholder) {
  color: #94a3b8;
}

.admin-permission-upsert-drawer__form :deep(.el-input__inner),
.admin-permission-upsert-drawer__form :deep(.el-select__selected-item) {
  color: #0f172a;
}

.admin-permission-upsert-drawer__form :deep(.el-textarea__inner) {
  border-radius: 14px;
  background: rgba(248, 250, 252, 0.92);
  box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.2);
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.admin-permission-upsert-drawer__form :deep(.el-textarea__inner:hover) {
  background: #ffffff;
  box-shadow: inset 0 0 0 1px rgba(100, 116, 139, 0.34);
}

.admin-permission-upsert-drawer__form :deep(.el-textarea__inner:focus) {
  background: #ffffff;
  box-shadow:
    inset 0 0 0 1px rgba(59, 130, 246, 0.64),
    0 0 0 4px rgba(59, 130, 246, 0.12);
}

.admin-permission-upsert-drawer__radio-group {
  display: flex;
  flex-wrap: wrap;
  gap: 12px 16px;
}

.admin-permission-upsert-drawer__radio-group :deep(.el-radio) {
  margin-right: 0;
  color: #334155;
  font-weight: 600;
}

.admin-permission-upsert-drawer__form :deep(.el-switch) {
  --el-switch-on-color: #2563eb;
}

.admin-permission-upsert-drawer__footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.admin-permission-upsert-drawer__footer :deep(.el-button) {
  min-width: 104px;
  min-height: 42px;
  border-radius: 14px;
  font-weight: 600;
}

@media (max-width: 640px) {
  .admin-permission-upsert-drawer :deep(.el-drawer__header) {
    padding: 18px 16px 14px;
  }

  .admin-permission-upsert-drawer :deep(.el-drawer__body) {
    padding: 18px 16px 14px;
  }

  .admin-permission-upsert-drawer :deep(.el-drawer__footer) {
    padding: 14px 16px 18px;
  }

  .admin-permission-upsert-drawer__footer {
    flex-wrap: wrap;
  }

  .admin-permission-upsert-drawer__footer :deep(.el-button) {
    flex: 1 1 140px;
  }
}
</style>
