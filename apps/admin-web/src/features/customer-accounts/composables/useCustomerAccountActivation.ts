import { ElMessage } from 'element-plus'
import { reactive } from 'vue'

import {
  disableCustomerAccount,
  enableCustomerAccount,
} from '@/features/customer-accounts/api/customerAccountsApi'
import {
  canDisableCustomerAccount,
  canEnableCustomerAccount,
} from '@/features/customer-accounts/models/customerAccountsList'
import type { CustomerAccountBasicResponse } from '@/features/customer-accounts/types/customerAccounts'
import type { CustomerAccountActivationAction } from '@/features/customer-accounts/models/customerAccountsList'
import { extractErrorMessage } from '@/shared/api/error'

export function useCustomerAccountActivation(options: {
  refreshCustomers: () => Promise<unknown> | void
}) {
  const activationState = reactive<{
    action: CustomerAccountActivationAction | null
    customerId: string | null
  }>({
    action: null,
    customerId: null,
  })

  function canEnableRow(row: CustomerAccountBasicResponse) {
    return canEnableCustomerAccount(row)
  }

  function canDisableRow(row: CustomerAccountBasicResponse) {
    return canDisableCustomerAccount(row)
  }

  function isRowActivationPending(customerId: string, action?: CustomerAccountActivationAction) {
    if (activationState.customerId !== customerId) {
      return false
    }

    if (!action) {
      return true
    }

    return activationState.action === action
  }

  async function handleActivation(
    row: CustomerAccountBasicResponse,
    action: CustomerAccountActivationAction,
  ) {
    activationState.customerId = row.id
    activationState.action = action

    try {
      if (action === 'enable') {
        await enableCustomerAccount(row.id)
        ElMessage.success('启用客户成功')
      } else {
        await disableCustomerAccount(row.id)
        ElMessage.success('禁用客户成功')
      }

      await options.refreshCustomers()
    } catch (error) {
      ElMessage.error(
        extractErrorMessage(error, action === 'enable' ? '启用客户失败' : '禁用客户失败'),
      )
    } finally {
      activationState.customerId = null
      activationState.action = null
    }
  }

  async function handleEnable(row: CustomerAccountBasicResponse) {
    await handleActivation(row, 'enable')
  }

  async function handleDisable(row: CustomerAccountBasicResponse) {
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
