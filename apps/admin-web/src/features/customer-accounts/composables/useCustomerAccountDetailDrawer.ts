import { ElMessage } from 'element-plus'
import { ref, watch } from 'vue'

import { getCustomerAccountById } from '@/features/customer-accounts/api/customerAccountsApi'
import type {
  CustomerAccountBasicResponse,
  CustomerAccountDetailResponse,
} from '@/features/customer-accounts/types/customerAccounts'
import { extractErrorMessage } from '@/shared/api/error'

export function useCustomerAccountDetailDrawer() {
  const isDetailDrawerOpen = ref(false)
  const selectedCustomer = ref<CustomerAccountBasicResponse | null>(null)
  const detail = ref<CustomerAccountDetailResponse | null>(null)
  const isLoading = ref(false)

  function handleOpenDetailDrawer(row: CustomerAccountBasicResponse) {
    selectedCustomer.value = row
    isDetailDrawerOpen.value = true
  }

  function handleCloseDetailDrawer() {
    isDetailDrawerOpen.value = false
  }

  async function loadDetail(customerId: string) {
    isLoading.value = true

    try {
      detail.value = await getCustomerAccountById(customerId)
    } catch (error) {
      ElMessage.error(extractErrorMessage(error, '加载客户详情失败'))
      handleCloseDetailDrawer()
    } finally {
      isLoading.value = false
    }
  }

  watch(
    () => [isDetailDrawerOpen.value, selectedCustomer.value?.id] as const,
    ([isOpen, customerId]) => {
      if (!isOpen || !customerId) {
        return
      }

      void loadDetail(customerId)
    },
  )

  watch(isDetailDrawerOpen, (isOpen) => {
    if (isOpen) {
      return
    }

    selectedCustomer.value = null
    detail.value = null
    isLoading.value = false
  })

  return {
    detail,
    handleCloseDetailDrawer,
    handleOpenDetailDrawer,
    isDetailDrawerOpen,
    isLoading,
    selectedCustomer,
  }
}
