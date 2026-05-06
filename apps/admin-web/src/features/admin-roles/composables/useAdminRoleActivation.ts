import { ElMessage } from 'element-plus'
import { reactive } from 'vue'

import {
  disableAdminRole,
  enableAdminRole,
} from '@/features/admin-roles/api/adminRolesApi'
import {
  canDisableAdminRole,
  canEnableAdminRole,
} from '@/features/admin-roles/models/adminRolesList'
import type { AdminRoleActivationAction } from '@/features/admin-roles/models/adminRolesList'
import type { AdminRoleBasicResponse } from '@/features/admin-roles/types/adminRoles'
import { extractErrorMessage } from '@/shared/api/error'

export function useAdminRoleActivation(options: {
  refreshRoles: () => Promise<unknown> | void
}) {
  const activationState = reactive<{
    action: AdminRoleActivationAction | null
    roleId: string | null
  }>({
    action: null,
    roleId: null,
  })

  function canEnableRow(row: AdminRoleBasicResponse) {
    return canEnableAdminRole(row)
  }

  function canDisableRow(row: AdminRoleBasicResponse) {
    return canDisableAdminRole(row)
  }

  function isRowActivationPending(roleId: string, action?: AdminRoleActivationAction) {
    if (activationState.roleId !== roleId) {
      return false
    }

    if (!action) {
      return true
    }

    return activationState.action === action
  }

  async function handleActivation(
    row: AdminRoleBasicResponse,
    action: AdminRoleActivationAction,
  ) {
    activationState.roleId = row.id
    activationState.action = action

    try {
      if (action === 'enable') {
        await enableAdminRole(row.id)
        ElMessage.success('启用角色成功')
      } else {
        await disableAdminRole(row.id)
        ElMessage.success('禁用角色成功')
      }

      await options.refreshRoles()
    } catch (error) {
      ElMessage.error(
        extractErrorMessage(error, action === 'enable' ? '启用角色失败' : '禁用角色失败'),
      )
    } finally {
      activationState.roleId = null
      activationState.action = null
    }
  }

  async function handleEnable(row: AdminRoleBasicResponse) {
    await handleActivation(row, 'enable')
  }

  async function handleDisable(row: AdminRoleBasicResponse) {
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
