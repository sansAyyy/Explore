<script setup lang="ts">
import {
  formatAdminUsersStatus,
  resolveAdminUsersStatusType,
} from '@/features/admin-users/models/adminUsersList'
import type {
  AdminUserActivationAction,
} from '@/features/admin-users/models/adminUsersList'
import type {
  AdminUserBasicResponse,
  AdminUsersPagination,
} from '@/features/admin-users/types/adminUsers'
import { formatDateTime } from '@/shared/utils/dateTime'

const props = defineProps<{
  canAssignAdminUserRoles: boolean
  canChangeAdminUserPassword: boolean
  canDisableRow: (row: AdminUserBasicResponse) => boolean
  canEnableRow: (row: AdminUserBasicResponse) => boolean
  canUpdateAdminUser: boolean
  currentUserId: string | null | undefined
  isLoading: boolean
  isRowActivationPending: (
    userId: string,
    action?: AdminUserActivationAction,
  ) => boolean
  pagination: AdminUsersPagination
  totalCount: number
  users: AdminUserBasicResponse[]
}>()

const emit = defineEmits<{
  'assign-roles': [row: AdminUserBasicResponse]
  'change-password': [row: AdminUserBasicResponse]
  disable: [row: AdminUserBasicResponse]
  edit: [row: AdminUserBasicResponse]
  enable: [row: AdminUserBasicResponse]
  'page-change': [pageIndex: number]
  'page-size-change': [pageSize: number]
}>()

function isCurrentAdminUser(row: AdminUserBasicResponse) {
  return row.id === props.currentUserId
}
</script>

<template>
  <el-card shadow="never" class="admin-users-table">
    <template #header>
      <div class="admin-users-table__header">
        <div>
          <div class="admin-users-table__title">用户列表</div>
          <p class="admin-users-table__subtitle">共 {{ totalCount }} 条记录</p>
        </div>
      </div>
    </template>

    <el-skeleton :loading="isLoading" animated class="admin-users-table__skeleton-shell">
      <template #template>
        <div class="admin-users-table__skeleton">
          <el-skeleton-item
            v-for="index in 6"
            :key="index"
            variant="rect"
            class="admin-users-table__skeleton-row"
          />
        </div>
      </template>

      <template #default>
        <div v-if="users.length === 0" class="admin-users-table__empty">
          <el-empty description="暂无用户数据" />
        </div>

        <template v-else>
          <div class="admin-users-table__table-shell">
            <el-table :data="users" stripe height="100%">
              <el-table-column prop="userName" label="账号" min-width="160" />
              <el-table-column prop="displayName" label="显示名称" min-width="160" />
              <el-table-column prop="email" label="邮箱" min-width="220" />
              <el-table-column prop="phoneNumber" label="手机号" min-width="160">
                <template #default="{ row }">
                  {{ row.phoneNumber || '-' }}
                </template>
              </el-table-column>
              <el-table-column prop="isActive" label="状态" width="100">
                <template #default="{ row }">
                  <el-tag :type="resolveAdminUsersStatusType(row.isActive)">
                    {{ formatAdminUsersStatus(row.isActive) }}
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column prop="createdAt" label="创建时间" min-width="180">
                <template #default="{ row }">
                  {{ formatDateTime(row.createdAt) }}
                </template>
              </el-table-column>
              <el-table-column prop="lastLoginAt" label="最近登录" min-width="180">
                <template #default="{ row }">
                  {{ formatDateTime(row.lastLoginAt) }}
                </template>
              </el-table-column>
              <el-table-column
                v-if="canUpdateAdminUser || canChangeAdminUserPassword || canAssignAdminUserRoles"
                label="操作"
                fixed="right"
                width="380"
              >
                <template #default="{ row }">
                  <div class="admin-users-table__row-actions">
                    <el-button
                      v-if="canUpdateAdminUser"
                      link
                      type="primary"
                      :disabled="isRowActivationPending(row.id)"
                      @click="emit('edit', row)"
                    >
                      编辑
                    </el-button>

                    <el-popconfirm
                      v-if="canUpdateAdminUser && canEnableRow(row)"
                      title="确认启用该管理员吗？"
                      confirm-button-text="确认"
                      cancel-button-text="取消"
                      :disabled="isRowActivationPending(row.id)"
                      @confirm="emit('enable', row)"
                    >
                      <template #reference>
                        <el-button
                          link
                          type="success"
                          :loading="isRowActivationPending(row.id, 'enable')"
                        >
                          启用
                        </el-button>
                      </template>
                    </el-popconfirm>

                    <el-popconfirm
                      v-if="canUpdateAdminUser && canDisableRow(row)"
                      title="确认禁用该管理员吗？"
                      confirm-button-text="确认"
                      cancel-button-text="取消"
                      :disabled="isRowActivationPending(row.id)"
                      @confirm="emit('disable', row)"
                    >
                      <template #reference>
                        <el-button
                          link
                          type="danger"
                          :loading="isRowActivationPending(row.id, 'disable')"
                        >
                          禁用
                        </el-button>
                      </template>
                    </el-popconfirm>

                    <el-button
                      v-if="canChangeAdminUserPassword"
                      link
                      type="warning"
                      :disabled="isRowActivationPending(row.id)"
                      @click="emit('change-password', row)"
                    >
                      修改密码
                    </el-button>

                    <el-button
                      v-if="canAssignAdminUserRoles"
                      link
                      type="primary"
                      :disabled="isRowActivationPending(row.id)"
                      @click="emit('assign-roles', row)"
                    >
                      分配角色
                    </el-button>

                    <span
                      v-if="row.isActive && isCurrentAdminUser(row)"
                      class="admin-users-table__row-tip"
                    >
                      当前登录用户
                    </span>
                  </div>
                </template>
              </el-table-column>
            </el-table>
          </div>

          <div class="admin-users-table__pagination">
            <el-pagination
              background
              layout="total, sizes, prev, pager, next"
              :current-page="pagination.pageIndex"
              :page-size="pagination.pageSize"
              :page-sizes="[10, 20, 50]"
              :total="totalCount"
              @current-change="emit('page-change', $event)"
              @size-change="emit('page-size-change', $event)"
            />
          </div>
        </template>
      </template>
    </el-skeleton>
  </el-card>
</template>

<style scoped>
.admin-users-table {
  display: flex;
  flex-direction: column;
  min-width: 0;
  min-height: 0;
  height: 100%;
  border: 1px solid rgba(148, 163, 184, 0.24);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.96);
  box-shadow:
    0 12px 24px rgba(15, 23, 42, 0.04),
    0 4px 10px rgba(59, 130, 246, 0.05);
  backdrop-filter: blur(10px);
}

.admin-users-table :deep(.el-card__header) {
  padding: 10px 14px 0;
  border-bottom: none;
}

.admin-users-table :deep(.el-card__body) {
  display: flex;
  flex: 1 1 auto;
  flex-direction: column;
  min-height: 0;
  padding: 8px 14px 12px;
}

.admin-users-table__header {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  align-items: center;
  padding-bottom: 4px;
}

.admin-users-table__title {
  color: #0f172a;
  font-size: 13px;
  font-weight: 700;
  line-height: 1.25;
}

.admin-users-table__subtitle {
  margin: 2px 0 0;
  color: #64748b;
  font-size: 11px;
  line-height: 1.4;
}

.admin-users-table__skeleton-shell {
  display: flex;
  flex: 1 1 auto;
  flex-direction: column;
  min-height: 0;
}

.admin-users-table__skeleton {
  display: grid;
  gap: 8px;
}

.admin-users-table__skeleton-row {
  width: 100%;
  height: 42px;
  border-radius: 12px;
}

.admin-users-table__table-shell {
  flex: 1 1 auto;
  min-width: 0;
  min-height: 0;
  overflow: hidden;
  padding: 3px;
  border: 1px solid rgba(148, 163, 184, 0.16);
  border-radius: 14px;
  background:
    linear-gradient(180deg, rgba(248, 250, 252, 0.92), rgba(255, 255, 255, 0.97));
}

.admin-users-table__table-shell :deep(.el-table) {
  min-width: 0;
  width: 100%;
  height: 100%;
  border-radius: 12px;
  color: #0f172a;
  --el-table-border-color: rgba(226, 232, 240, 0.95);
  --el-table-header-bg-color: rgba(241, 245, 249, 0.96);
  --el-table-row-hover-bg-color: rgba(239, 246, 255, 0.92);
  --el-table-tr-bg-color: rgba(255, 255, 255, 0.96);
}

.admin-users-table__table-shell :deep(.el-scrollbar) {
  height: 100%;
}

.admin-users-table__table-shell :deep(.el-scrollbar__wrap) {
  overflow-x: auto;
  overflow-y: auto;
}

.admin-users-table__table-shell :deep(.el-table__inner-wrapper) {
  min-width: 1180px;
}

.admin-users-table__table-shell :deep(.el-table th.el-table__cell) {
  padding: 8px 0;
  background: rgba(241, 245, 249, 0.96);
}

.admin-users-table__table-shell :deep(.el-table th.el-table__cell > .cell) {
  color: #334155;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
}

.admin-users-table__table-shell :deep(.el-table .el-table__cell) {
  padding: 8px 0;
}

.admin-users-table__table-shell :deep(.el-table .cell) {
  color: #0f172a;
  line-height: 1.45;
}

.admin-users-table__table-shell :deep(.el-table__row--striped td.el-table__cell) {
  background: rgba(248, 250, 252, 0.82);
}

.admin-users-table__table-shell :deep(.el-table__body-wrapper) {
  overflow-y: auto;
}

.admin-users-table__table-shell :deep(.el-tag) {
  min-height: 26px;
  padding: 0 10px;
  border-radius: 999px;
  font-weight: 600;
}

.admin-users-table__table-shell :deep(.el-button.is-link) {
  font-weight: 600;
}

.admin-users-table__table-shell :deep(.el-table__fixed-right::before),
.admin-users-table__table-shell :deep(.el-table__fixed::before) {
  background-color: rgba(226, 232, 240, 0.95);
}

.admin-users-table__row-actions {
  display: inline-flex;
  align-items: center;
  gap: 6px 8px;
  flex-wrap: wrap;
}

.admin-users-table__row-tip {
  display: inline-flex;
  align-items: center;
  min-height: 24px;
  padding: 0 8px;
  border-radius: 999px;
  background: rgba(219, 234, 254, 0.82);
  color: #1d4ed8;
  font-size: 11px;
  font-weight: 600;
  white-space: nowrap;
}

.admin-users-table__empty {
  display: grid;
  flex: 1 1 auto;
  min-height: 0;
  place-items: center;
  padding: 12px;
  border: 1px dashed rgba(148, 163, 184, 0.28);
  border-radius: 14px;
  background: linear-gradient(180deg, rgba(248, 250, 252, 0.72), rgba(255, 255, 255, 0.94));
}

.admin-users-table__empty :deep(.el-empty__description p) {
  color: #64748b;
}

.admin-users-table__pagination {
  display: flex;
  flex: none;
  justify-content: flex-end;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 8px;
  padding-top: 8px;
  border-top: 1px solid rgba(226, 232, 240, 0.92);
}

.admin-users-table__pagination :deep(.el-pagination) {
  flex-wrap: wrap;
  gap: 8px;
}

.admin-users-table__pagination :deep(.btn-prev),
.admin-users-table__pagination :deep(.btn-next),
.admin-users-table__pagination :deep(.el-pager li) {
  border-radius: 10px;
}

@media (max-width: 920px) {
  .admin-users-table {
    height: auto;
    min-height: 0;
  }

  .admin-users-table :deep(.el-card__header) {
    padding: 12px 14px 0;
  }

  .admin-users-table :deep(.el-card__body) {
    padding: 10px 14px 14px;
  }

  .admin-users-table__header {
    align-items: flex-start;
    flex-direction: column;
  }

  .admin-users-table__table-shell {
    flex: none;
    min-height: 0;
    overflow-y: auto;
  }

  .admin-users-table__pagination {
    justify-content: flex-start;
  }
}

@media (max-width: 640px) {
  .admin-users-table {
    border-radius: 16px;
  }

  .admin-users-table :deep(.el-card__header) {
    padding: 10px 12px 0;
  }

  .admin-users-table :deep(.el-card__body) {
    padding: 8px 12px 12px;
  }

  .admin-users-table__table-shell,
  .admin-users-table__empty {
    border-radius: 14px;
  }
}
</style>
