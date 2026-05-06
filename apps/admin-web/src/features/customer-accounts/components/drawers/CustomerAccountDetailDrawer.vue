<script setup lang="ts">
import {
  formatCustomerAccountsStatus,
} from '@/features/customer-accounts/models/customerAccountsList'
import type {
  CustomerAccountBasicResponse,
  CustomerAccountDetailResponse,
} from '@/features/customer-accounts/types/customerAccounts'
import { formatDateTime } from '@/shared/utils/dateTime'

defineProps<{
  customer: CustomerAccountBasicResponse | null
  detail: CustomerAccountDetailResponse | null
  isLoading: boolean
  modelValue: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

function closeDrawer() {
  emit('update:modelValue', false)
}
</script>

<template>
  <el-drawer
    :model-value="modelValue"
    title="客户详情"
    size="640px"
    destroy-on-close
    class="customer-account-detail-drawer"
    @close="closeDrawer"
  >
    <div v-loading="isLoading" class="customer-account-detail-drawer__panel">
      <template v-if="detail">
        <div class="customer-account-detail-drawer__hero">
          <el-avatar
            :src="detail.avatarUrl || undefined"
            :size="56"
            class="customer-account-detail-drawer__avatar"
          >
            {{ detail.nickName.slice(0, 1).toUpperCase() }}
          </el-avatar>
          <div class="customer-account-detail-drawer__summary">
            <strong>{{ detail.nickName }}</strong>
            <span>{{ detail.phoneNumber }}</span>
            <el-tag size="small" :type="detail.isActive ? 'success' : 'info'">
              {{ formatCustomerAccountsStatus(detail.isActive) }}
            </el-tag>
          </div>
        </div>

        <el-descriptions :column="1" border class="customer-account-detail-drawer__descriptions">
          <el-descriptions-item label="手机号">
            {{ detail.phoneNumber }}
          </el-descriptions-item>
          <el-descriptions-item label="昵称">
            {{ detail.nickName }}
          </el-descriptions-item>
          <el-descriptions-item label="邮箱">
            {{ detail.email || '-' }}
          </el-descriptions-item>
          <el-descriptions-item label="头像地址">
            {{ detail.avatarUrl || '-' }}
          </el-descriptions-item>
          <el-descriptions-item label="创建时间">
            {{ formatDateTime(detail.createdAt) }}
          </el-descriptions-item>
          <el-descriptions-item label="创建人">
            {{ detail.createdBy }}
          </el-descriptions-item>
          <el-descriptions-item label="更新时间">
            {{ formatDateTime(detail.updatedAt) }}
          </el-descriptions-item>
          <el-descriptions-item label="更新人">
            {{ detail.updatedBy || '-' }}
          </el-descriptions-item>
          <el-descriptions-item label="最近登录">
            {{ formatDateTime(detail.lastLoginAt) }}
          </el-descriptions-item>
          <el-descriptions-item label="版本号">
            {{ detail.version }}
          </el-descriptions-item>
        </el-descriptions>
      </template>

      <div v-else class="customer-account-detail-drawer__empty">
        <el-empty :description="customer ? '未加载到客户详情' : '请选择客户'" />
      </div>
    </div>
  </el-drawer>
</template>

<style scoped>
.customer-account-detail-drawer :deep(.el-drawer) {
  background: linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.customer-account-detail-drawer :deep(.el-drawer__header) {
  margin-bottom: 0;
  padding: 22px 24px 18px;
  border-bottom: 1px solid rgba(226, 232, 240, 0.92);
}

.customer-account-detail-drawer :deep(.el-drawer__title) {
  color: #0f172a;
  font-size: 18px;
  font-weight: 700;
  letter-spacing: -0.02em;
}

.customer-account-detail-drawer :deep(.el-drawer__body) {
  padding: 22px 24px 18px;
}

.customer-account-detail-drawer__panel {
  display: grid;
  gap: 18px;
  min-height: 0;
}

.customer-account-detail-drawer__hero {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 18px;
  border: 1px solid rgba(148, 163, 184, 0.2);
  border-radius: 18px;
  background: linear-gradient(135deg, rgba(30, 64, 175, 0.08), rgba(59, 130, 246, 0.03));
}

.customer-account-detail-drawer__avatar {
  flex: none;
}

.customer-account-detail-drawer__summary {
  display: grid;
  gap: 6px;
}

.customer-account-detail-drawer__summary strong {
  color: #0f172a;
  font-size: 18px;
  line-height: 1.2;
}

.customer-account-detail-drawer__summary span {
  color: #64748b;
}

.customer-account-detail-drawer__summary :deep(.el-tag) {
  width: fit-content;
  min-height: 24px;
  padding: 0 8px;
  border-radius: 999px;
  font-weight: 600;
}

.customer-account-detail-drawer__descriptions :deep(.el-descriptions__body) {
  border-radius: 16px;
  overflow: hidden;
}

.customer-account-detail-drawer__descriptions :deep(.el-descriptions__label) {
  width: 120px;
  color: #334155;
  font-weight: 600;
  background: rgba(248, 250, 252, 0.9);
}

.customer-account-detail-drawer__descriptions :deep(.el-descriptions__content) {
  color: #0f172a;
  background: rgba(255, 255, 255, 0.96);
}

.customer-account-detail-drawer__empty {
  display: grid;
  place-items: center;
  min-height: 240px;
  padding: 12px;
  border: 1px dashed rgba(148, 163, 184, 0.28);
  border-radius: 14px;
  background: linear-gradient(180deg, rgba(248, 250, 252, 0.72), rgba(255, 255, 255, 0.94));
}

.customer-account-detail-drawer__empty :deep(.el-empty__description p) {
  color: #64748b;
}

@media (max-width: 640px) {
  .customer-account-detail-drawer :deep(.el-drawer__header) {
    padding: 18px 16px 14px;
  }

  .customer-account-detail-drawer :deep(.el-drawer__body) {
    padding: 18px 16px 14px;
  }

  .customer-account-detail-drawer__hero {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
