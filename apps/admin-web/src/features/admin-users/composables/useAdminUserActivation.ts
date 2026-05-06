import { ElMessage } from 'element-plus'
import { reactive } from 'vue'
import type { Ref } from 'vue'

import {
  disableAdminUser,
  enableAdminUser,
} from '@/features/admin-users/api/adminUsersApi'
import {
  canDisableAdminUser,
  canEnableAdminUser,
} from '@/features/admin-users/models/adminUsersList'
import type {
  AdminUserActivationAction,
} from '@/features/admin-users/models/adminUsersList'
import type { AdminUserBasicResponse } from '@/features/admin-users/types/adminUsers'
import { extractErrorMessage } from '@/shared/api/error'

export function useAdminUserActivation(options: {
  currentUserId: Ref<string | null | undefined>
  refreshUsers: () => Promise<unknown> | void
}) {
  const activationState = reactive<{
    action: AdminUserActivationAction | null
    userId: string | null
  }>({
    action: null,
    userId: null,
  })

  function canEnableRow(row: AdminUserBasicResponse) {
    return canEnableAdminUser(row)
  }

  function canDisableRow(row: AdminUserBasicResponse) {
    return canDisableAdminUser(row, options.currentUserId.value)
  }

  function isRowActivationPending(userId: string, action?: AdminUserActivationAction) {
    if (activationState.userId !== userId) {
      return false
    }

    if (!action) {
      return true
    }

    return activationState.action === action
  }

  async function handleActivation(
    row: AdminUserBasicResponse,
    action: AdminUserActivationAction,
  ) {
    activationState.userId = row.id
    activationState.action = action

    try {
      if (action === 'enable') {
        await enableAdminUser(row.id)
        ElMessage.success('启用管理员成功')
      } else {
        await disableAdminUser(row.id)
        ElMessage.success('禁用管理员成功')
      }

      await options.refreshUsers()
    } catch (error) {
      ElMessage.error(
        extractErrorMessage(error, action === 'enable' ? '启用管理员失败' : '禁用管理员失败'),
      )
    } finally {
      activationState.userId = null
      activationState.action = null
    }
  }

  async function handleEnable(row: AdminUserBasicResponse) {
    await handleActivation(row, 'enable')
  }

  async function handleDisable(row: AdminUserBasicResponse) {
    await handleActivation(row, 'disable')
  }

  return {
    canDisableRow,
    canEnableRow,
    handleDisable,
    handleEnable,
    isRowActivationPending,
  }
}
