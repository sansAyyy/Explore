import { ElMessage } from 'element-plus'
import { computed, reactive, ref } from 'vue'

import { getPagedCustomerAccounts } from '@/features/customer-accounts/api/customerAccountsApi'
import {
  buildCustomerAccountsQuery,
  createDefaultCustomerAccountsFilters,
  createDefaultCustomerAccountsPagination,
  customerAccountsStatusOptions,
} from '@/features/customer-accounts/models/customerAccountsList'
import type { CustomerAccountBasicResponse } from '@/features/customer-accounts/types/customerAccounts'
import { extractErrorMessage } from '@/shared/api/error'

export function useCustomerAccountsList() {
  const customers = ref<CustomerAccountBasicResponse[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const filters = reactive(createDefaultCustomerAccountsFilters())
  const pagination = reactive(createDefaultCustomerAccountsPagination())
  const querySummary = computed(() => buildCustomerAccountsQuery(filters, pagination))

  async function loadCustomers() {
    isLoading.value = true

    try {
      const result = await getPagedCustomerAccounts(querySummary.value)
      customers.value = result.items
      totalCount.value = result.totalCount
    } catch (error) {
      customers.value = []
      totalCount.value = 0
      ElMessage.error(extractErrorMessage(error, '加载客户列表失败'))
    } finally {
      isLoading.value = false
    }
  }

  async function handleSearch() {
    pagination.pageIndex = 1
    await loadCustomers()
  }

  async function handleReset() {
    Object.assign(filters, createDefaultCustomerAccountsFilters())
    Object.assign(pagination, createDefaultCustomerAccountsPagination())
    await loadCustomers()
  }

  async function handlePageChange(pageIndex: number) {
    pagination.pageIndex = pageIndex
    await loadCustomers()
  }

  async function handlePageSizeChange(pageSize: number) {
    pagination.pageSize = pageSize
    pagination.pageIndex = 1
    await loadCustomers()
  }

  return {
    customers,
    filters,
    handlePageChange,
    handlePageSizeChange,
    handleReset,
    handleSearch,
    isLoading,
    loadCustomers,
    pagination,
    querySummary,
    statusOptions: customerAccountsStatusOptions,
    totalCount,
  }
}
