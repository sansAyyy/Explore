<script setup lang="ts">
import { Plus } from '@element-plus/icons-vue'

import type {
  MessageTemplatesPageFilters,
  MessageTemplatesStatusFilter,
} from '@/features/message-templates/types/messageTemplates'

defineProps<{
  canCreateMessageTemplate: boolean
  filters: MessageTemplatesPageFilters
  statusOptions: Array<{ label: string; value: MessageTemplatesStatusFilter }>
}>()

const emit = defineEmits<{
  create: []
  reset: []
  search: []
}>()
</script>

<template>
  <el-card shadow="never" class="message-templates-filters-card">
    <template #header>
      <div class="message-templates-filters-card__header">
        <div class="message-templates-filters-card__title">筛选条件</div>
      </div>
    </template>

    <div class="message-templates-filters-card__toolbar">
      <div class="message-templates-filters-card__fields">
        <el-input
          v-model="filters.keyword"
          clearable
          placeholder="按模板编码、名称或描述搜索"
          class="message-templates-filters-card__field message-templates-filters-card__field--keyword"
        />

        <el-select
          v-model="filters.status"
          class="message-templates-filters-card__field message-templates-filters-card__field--status"
          placeholder="请选择状态"
        >
          <el-option
            v-for="option in statusOptions"
            :key="option.value"
            :label="option.label"
            :value="option.value"
          />
        </el-select>
      </div>

      <div class="message-templates-filters-card__actions">
        <el-button
          v-if="canCreateMessageTemplate"
          type="primary"
          :icon="Plus"
          class="message-templates-filters-card__button message-templates-filters-card__button--primary"
          @click="emit('create')"
        >
          新增模板
        </el-button>
        <el-button
          type="primary"
          plain
          class="message-templates-filters-card__button message-templates-filters-card__button--secondary"
          @click="emit('search')"
        >
          查询
        </el-button>
        <el-button
          class="message-templates-filters-card__button message-templates-filters-card__button--ghost"
          @click="emit('reset')"
        >
          重置
        </el-button>
      </div>
    </div>
  </el-card>
</template>

<style scoped>
.message-templates-filters-card {
  border: 1px solid rgba(148, 163, 184, 0.22);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.95);
  box-shadow:
    0 12px 24px rgba(15, 23, 42, 0.04),
    0 4px 10px rgba(59, 130, 246, 0.05);
  backdrop-filter: blur(10px);
}

.message-templates-filters-card :deep(.el-card__header) {
  padding: 10px 14px 0;
  border-bottom: none;
}

.message-templates-filters-card :deep(.el-card__body) {
  padding: 8px 14px 12px;
}

.message-templates-filters-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.message-templates-filters-card__title {
  color: #0f172a;
  font-size: 12px;
  font-weight: 700;
  line-height: 1.25;
}

.message-templates-filters-card__toolbar {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 10px;
  align-items: center;
}

.message-templates-filters-card__fields {
  display: grid;
  grid-template-columns: minmax(240px, 1fr) minmax(150px, 180px);
  gap: 10px;
  align-items: center;
}

.message-templates-filters-card__field {
  width: 100%;
}

.message-templates-filters-card__field :deep(.el-input__wrapper),
.message-templates-filters-card__field :deep(.el-select__wrapper) {
  min-height: 38px;
  padding: 0 10px;
  border-radius: 10px;
  background: rgba(248, 250, 252, 0.94);
  box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.18);
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.message-templates-filters-card__field :deep(.el-input__inner),
.message-templates-filters-card__field :deep(.el-select__placeholder),
.message-templates-filters-card__field :deep(.el-select__selected-item) {
  color: #0f172a;
}

.message-templates-filters-card__field :deep(.el-input__inner::placeholder) {
  color: #94a3b8;
}

.message-templates-filters-card__field :deep(.el-input__wrapper:hover),
.message-templates-filters-card__field :deep(.el-select__wrapper:hover) {
  background: #ffffff;
  box-shadow: inset 0 0 0 1px rgba(100, 116, 139, 0.28);
}

.message-templates-filters-card__field :deep(.el-input__wrapper.is-focus),
.message-templates-filters-card__field :deep(.el-select__wrapper.is-focused) {
  background: #ffffff;
  box-shadow:
    inset 0 0 0 1px rgba(59, 130, 246, 0.6),
    0 0 0 3px rgba(59, 130, 246, 0.1);
}

.message-templates-filters-card__actions {
  display: flex;
  align-items: center;
  gap: 8px;
  justify-content: flex-end;
}

.message-templates-filters-card__button {
  min-width: 86px;
  min-height: 38px;
  padding: 0 14px;
  border-radius: 10px;
  font-weight: 600;
  transition:
    border-color 0.2s ease,
    background-color 0.2s ease,
    color 0.2s ease,
    box-shadow 0.2s ease;
}

.message-templates-filters-card__button--primary {
  border-color: transparent;
  background: linear-gradient(135deg, #1e40af 0%, #2563eb 100%);
  box-shadow: 0 8px 18px rgba(37, 99, 235, 0.16);
}

.message-templates-filters-card__button--secondary {
  border-color: rgba(37, 99, 235, 0.2);
  background: rgba(239, 246, 255, 0.92);
  color: #1d4ed8;
}

.message-templates-filters-card__button--ghost {
  border-color: rgba(148, 163, 184, 0.24);
  background: rgba(248, 250, 252, 0.92);
  color: #334155;
}

@media (max-width: 920px) {
  .message-templates-filters-card :deep(.el-card__header) {
    padding: 12px 14px 0;
  }

  .message-templates-filters-card :deep(.el-card__body) {
    padding: 10px 14px 14px;
  }

  .message-templates-filters-card__toolbar {
    grid-template-columns: 1fr;
    align-items: stretch;
  }

  .message-templates-filters-card__actions {
    justify-content: flex-start;
    flex-wrap: wrap;
  }
}

@media (max-width: 720px) {
  .message-templates-filters-card__fields {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .message-templates-filters-card :deep(.el-card__header) {
    padding: 10px 12px 0;
  }

  .message-templates-filters-card :deep(.el-card__body) {
    padding: 8px 12px 12px;
  }

  .message-templates-filters-card__button {
    min-width: 0;
    flex: 1 1 120px;
  }
}
</style>
