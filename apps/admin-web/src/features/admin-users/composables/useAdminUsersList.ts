import { ElMessage } from 'element-plus'
import { computed, reactive, ref } from 'vue'

import { getPagedAdminUsers } from '@/features/admin-users/api/adminUsersApi'
import {
  adminUsersStatusOptions,
  buildAdminUsersQuery,
  createDefaultAdminUsersFilters,
  createDefaultAdminUsersPagination,
} from '@/features/admin-users/models/adminUsersList'
import type { AdminUserBasicResponse } from '@/features/admin-users/types/adminUsers'
import { extractErrorMessage } from '@/shared/api/error'

export function useAdminUsersList() {
  const users = ref<AdminUserBasicResponse[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const filters = reactive(createDefaultAdminUsersFilters())
  const pagination = reactive(createDefaultAdminUsersPagination())
  const querySummary = computed(() => buildAdminUsersQuery(filters, pagination))

  async function loadUsers() {
    isLoading.value = true

    try {
      const result = await getPagedAdminUsers(querySummary.value)
      users.value = result.items
      totalCount.value = result.totalCount
    } catch (error) {
      users.value = []
      totalCount.value = 0
      ElMessage.error(extractErrorMessage(error, '加载用户管理失败'))
    } finally {
      isLoading.value = false
    }
  }

  async function handleSearch() {
    pagination.pageIndex = 1
    await loadUsers()
  }

  async function handleReset() {
    Object.assign(filters, createDefaultAdminUsersFilters())
    Object.assign(pagination, createDefaultAdminUsersPagination())
    await loadUsers()
  }

  async function handlePageChange(pageIndex: number) {
    pagination.pageIndex = pageIndex
    await loadUsers()
  }

  async function handlePageSizeChange(pageSize: number) {
    pagination.pageSize = pageSize
    pagination.pageIndex = 1
    await loadUsers()
  }

  return {
    filters,
    handlePageChange,
    handlePageSizeChange,
    handleReset,
    handleSearch,
    isLoading,
    loadUsers,
    pagination,
    querySummary,
    statusOptions: adminUsersStatusOptions,
    totalCount,
    users,
  }
}
