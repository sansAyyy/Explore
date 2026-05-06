<script setup lang="ts">
import { toRef } from 'vue'

import { useAdminUserRoleAssignmentDrawer } from '@/features/admin-users/composables/useAdminUserRoleAssignmentDrawer'
import type { AdminUserBasicResponse } from '@/features/admin-users/types/adminUsers'

const props = defineProps<{
  modelValue: boolean
  user: AdminUserBasicResponse | null
}>()

const emit = defineEmits<{
  changed: []
  'update:modelValue': [value: boolean]
}>()

function closeDrawer() {
  emit('update:modelValue', false)
}

const {
  filters,
  handleReset,
  handleSearch,
  handleSubmit,
  isLoading,
  isSubmitting,
  roleOptions,
  selectedCount,
  selectedRoleIds,
  userDisplay,
} = useAdminUserRoleAssignmentDrawer({
  closeDrawer,
  modelValue: toRef(props, 'modelValue'),
  onChanged: () => emit('changed'),
  user: toRef(props, 'user'),
})
</script>

<template>
  <el-drawer
    :model-value="modelValue"
    title="分配角色"
    size="620px"
    destroy-on-close
    class="admin-user-assign-roles-drawer"
    @close="closeDrawer"
  >
    <div class="admin-user-assign-roles-drawer__panel">
      <div class="admin-user-assign-roles-drawer__context">
        <p class="admin-user-assign-roles-drawer__label">当前操作对象</p>
        <p class="admin-user-assign-roles-drawer__value">{{ userDisplay }}</p>
      </div>

      <el-card shadow="never" class="admin-user-assign-roles-drawer__search-card">
        <div class="admin-user-assign-roles-drawer__toolbar">
          <el-input
            v-model="filters.keyword"
            clearable
            placeholder="按角色名称或编码搜索"
            class="admin-user-assign-roles-drawer__field"
          />
          <div class="admin-user-assign-roles-drawer__actions">
            <el-button type="primary" @click="handleSearch">查询</el-button>
            <el-button @click="handleReset">重置</el-button>
          </div>
        </div>
      </el-card>

      <div class="admin-user-assign-roles-drawer__meta">已选 {{ selectedCount }} 个角色</div>

      <div v-loading="isLoading" class="admin-user-assign-roles-drawer__content">
        <el-empty
          v-if="!isLoading && roleOptions.length === 0"
          description="暂无可分配的启用角色"
        />

        <el-checkbox-group
          v-else
          v-model="selectedRoleIds"
          class="admin-user-assign-roles-drawer__list"
        >
          <el-checkbox
            v-for="option in roleOptions"
            :key="option.value"
            :value="option.value"
            class="admin-user-assign-roles-drawer__item"
          >
            <div class="admin-user-assign-roles-drawer__item-content">
              <span class="admin-user-assign-roles-drawer__item-label">{{ option.label }}</span>
            </div>
          </el-checkbox>
        </el-checkbox-group>
      </div>
    </div>

    <template #footer>
      <div class="admin-user-assign-roles-drawer__footer">
        <el-button @click="closeDrawer">取消</el-button>
        <el-button type="primary" :loading="isSubmitting" @click="handleSubmit">
          保存角色
        </el-button>
      </div>
    </template>
  </el-drawer>
</template>

<style scoped>
.admin-user-assign-roles-drawer :deep(.el-drawer) {
  background: linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.admin-user-assign-roles-drawer :deep(.el-drawer__header) {
  margin-bottom: 0;
  padding: 22px 24px 18px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.92);
}

.admin-user-assign-roles-drawer :deep(.el-drawer__title) {
  color: #0f172a;
  font-size: 18px;
  font-weight: 700;
  letter-spacing: -0.02em;
}

.admin-user-assign-roles-drawer :deep(.el-drawer__body) {
  padding: 22px 24px 18px;
}

.admin-user-assign-roles-drawer :deep(.el-drawer__footer) {
  padding: 16px 24px 22px;
  border-top: 1px solid rgba(226, 232, 240, 0.92);
  background: rgba(248, 250, 252, 0.88);
}

.admin-user-assign-roles-drawer__panel {
  display: grid;
  gap: 16px;
}

.admin-user-assign-roles-drawer__context {
  padding: 16px 18px;
  border: 1px solid rgba(191, 219, 254, 0.45);
  border-radius: 18px;
  background:
    linear-gradient(135deg, rgba(239, 246, 255, 0.92), rgba(255, 255, 255, 0.96));
  box-shadow: 0 10px 24px rgba(59, 130, 246, 0.08);
}

.admin-user-assign-roles-drawer__label {
  margin: 0 0 8px;
  color: #64748b;
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.admin-user-assign-roles-drawer__value {
  margin: 0;
  color: #0f172a;
  font-weight: 700;
  line-height: 1.5;
}

.admin-user-assign-roles-drawer__search-card {
  border: 1px solid rgba(148, 163, 184, 0.22);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.92);
  box-shadow: 0 12px 26px rgba(15, 23, 42, 0.04);
}

.admin-user-assign-roles-drawer__search-card :deep(.el-card__body) {
  padding: 16px;
}

.admin-user-assign-roles-drawer__toolbar {
  display: grid;
  grid-template-columns: minmax(220px, 1fr) auto;
  gap: 12px;
  align-items: center;
}

.admin-user-assign-roles-drawer__field {
  width: 100%;
}

.admin-user-assign-roles-drawer__field :deep(.el-input__wrapper) {
  min-height: 44px;
  border-radius: 14px;
  background: rgba(248, 250, 252, 0.92);
  box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.2);
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.admin-user-assign-roles-drawer__field :deep(.el-input__wrapper:hover) {
  background: #ffffff;
  box-shadow: inset 0 0 0 1px rgba(100, 116, 139, 0.34);
}

.admin-user-assign-roles-drawer__field :deep(.el-input__wrapper.is-focus) {
  background: #ffffff;
  box-shadow:
    inset 0 0 0 1px rgba(59, 130, 246, 0.64),
    0 0 0 4px rgba(59, 130, 246, 0.12);
}

.admin-user-assign-roles-drawer__field :deep(.el-input__inner::placeholder) {
  color: #94a3b8;
}

.admin-user-assign-roles-drawer__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.admin-user-assign-roles-drawer__actions :deep(.el-button) {
  min-width: 96px;
  min-height: 42px;
  border-radius: 14px;
  font-weight: 600;
}

.admin-user-assign-roles-drawer__meta {
  color: #64748b;
  font-size: 13px;
  font-weight: 600;
}

.admin-user-assign-roles-drawer__content {
  min-height: 240px;
}

.admin-user-assign-roles-drawer__content :deep(.el-loading-mask) {
  border-radius: 20px;
}

.admin-user-assign-roles-drawer__content :deep(.el-empty) {
  min-height: 220px;
  border: 1px dashed rgba(148, 163, 184, 0.28);
  border-radius: 20px;
  background: rgba(248, 250, 252, 0.72);
}

.admin-user-assign-roles-drawer__list {
  display: grid;
  gap: 12px;
}

.admin-user-assign-roles-drawer__item {
  display: flex;
  align-items: center;
  width: 100%;
  margin-right: 0;
  padding: 14px 16px;
  border: 1px solid rgba(226, 232, 240, 0.95);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.94);
  box-sizing: border-box;
  transition:
    border-color 0.2s ease,
    background-color 0.2s ease,
    box-shadow 0.2s ease,
    transform 0.2s ease;
}

.admin-user-assign-roles-drawer__item:hover {
  border-color: rgba(96, 165, 250, 0.42);
  background: rgba(248, 250, 252, 0.98);
  box-shadow: 0 12px 24px rgba(59, 130, 246, 0.08);
  transform: translateY(-1px);
}

.admin-user-assign-roles-drawer__item :deep(.el-checkbox__input) {
  display: flex;
  align-items: center;
  flex: 0 0 auto;
}

.admin-user-assign-roles-drawer__item :deep(.el-checkbox__label) {
  display: flex;
  align-items: center;
  width: 100%;
  padding-left: 12px;
  line-height: 1.5;
}

.admin-user-assign-roles-drawer__item :deep(.el-checkbox__input.is-checked + .el-checkbox__label) {
  color: inherit;
}

.admin-user-assign-roles-drawer__item :deep(.el-checkbox__input.is-checked .el-checkbox__inner) {
  background-color: #2563eb;
  border-color: #2563eb;
}

.admin-user-assign-roles-drawer__item:deep(.is-checked) {
  border-color: rgba(59, 130, 246, 0.46);
}

.admin-user-assign-roles-drawer__item-content {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.admin-user-assign-roles-drawer__item-label {
  color: #0f172a;
  font-weight: 700;
  line-height: 1.5;
}

.admin-user-assign-roles-drawer__item-code {
  color: #64748b;
  font-size: 12px;
  line-height: 1.5;
}

.admin-user-assign-roles-drawer__footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.admin-user-assign-roles-drawer__footer :deep(.el-button) {
  min-width: 104px;
  min-height: 42px;
  border-radius: 14px;
  font-weight: 600;
}

@media (max-width: 768px) {
  .admin-user-assign-roles-drawer__toolbar {
    grid-template-columns: 1fr;
  }

  .admin-user-assign-roles-drawer__actions {
    justify-content: flex-start;
  }
}

@media (max-width: 640px) {
  .admin-user-assign-roles-drawer :deep(.el-drawer__header) {
    padding: 18px 16px 14px;
  }

  .admin-user-assign-roles-drawer :deep(.el-drawer__body) {
    padding: 18px 16px 14px;
  }

  .admin-user-assign-roles-drawer :deep(.el-drawer__footer) {
    padding: 14px 16px 18px;
  }

  .admin-user-assign-roles-drawer__context,
  .admin-user-assign-roles-drawer__search-card,
  .admin-user-assign-roles-drawer__content :deep(.el-empty) {
    border-radius: 16px;
  }

  .admin-user-assign-roles-drawer__search-card :deep(.el-card__body) {
    padding: 14px;
  }

  .admin-user-assign-roles-drawer__footer {
    flex-wrap: wrap;
  }

  .admin-user-assign-roles-drawer__footer :deep(.el-button) {
    flex: 1 1 140px;
  }
}
</style>
