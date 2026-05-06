import { ElMessage } from 'element-plus'
import { computed, reactive, ref } from 'vue'

import { getPagedAdminRoles } from '@/features/admin-roles/api/adminRolesApi'
import {
  adminRolesStatusOptions,
  buildAdminRolesQuery,
  createDefaultAdminRolesFilters,
  createDefaultAdminRolesPagination,
} from '@/features/admin-roles/models/adminRolesList'
import type { AdminRoleBasicResponse } from '@/features/admin-roles/types/adminRoles'
import { extractErrorMessage } from '@/shared/api/error'

export function useAdminRolesList() {
  const roles = ref<AdminRoleBasicResponse[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const filters = reactive(createDefaultAdminRolesFilters())
  const pagination = reactive(createDefaultAdminRolesPagination())
  const querySummary = computed(() => buildAdminRolesQuery(filters, pagination))

  async function loadRoles() {
    isLoading.value = true

    try {
      const result = await getPagedAdminRoles(querySummary.value)
      roles.value = result.items
      totalCount.value = result.totalCount
    } catch (error) {
      roles.value = []
      totalCount.value = 0
      ElMessage.error(extractErrorMessage(error, '加载角色列表失败'))
    } finally {
      isLoading.value = false
    }
  }

  async function handleSearch() {
    pagination.pageIndex = 1
    await loadRoles()
  }

  async function handleReset() {
    Object.assign(filters, createDefaultAdminRolesFilters())
    Object.assign(pagination, createDefaultAdminRolesPagination())
    await loadRoles()
  }

  async function handlePageChange(pageIndex: number) {
    pagination.pageIndex = pageIndex
    await loadRoles()
  }

  async function handlePageSizeChange(pageSize: number) {
    pagination.pageSize = pageSize
    pagination.pageIndex = 1
    await loadRoles()
  }

  return {
    filters,
    handlePageChange,
    handlePageSizeChange,
    handleReset,
    handleSearch,
    isLoading,
    loadRoles,
    pagination,
    querySummary,
    roles,
    statusOptions: adminRolesStatusOptions,
    totalCount,
  }
}
