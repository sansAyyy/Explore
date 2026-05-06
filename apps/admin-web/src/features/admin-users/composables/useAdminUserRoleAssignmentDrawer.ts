import { ElMessage } from 'element-plus'
import { computed, reactive, ref, watch } from 'vue'
import type { Ref } from 'vue'

import {
  assignAdminUserRoles,
  getAdminUserRoles,
  getSelectableAdminRoles,
} from '@/features/admin-users/api/adminUsersApi'
import {
  buildAssignAdminUserRolesPayload,
  buildSelectableAdminRolesQuery,
  createDefaultAdminUserRoleSearchFilters,
  mapSelectableAdminRolesToOptions,
  resolveSelectedRoleIdsFromAssignments,
} from '@/features/admin-users/models/adminUserRoleAssignment'
import type {
  AdminUserBasicResponse,
  SelectableAdminRoleOption,
} from '@/features/admin-users/types/adminUsers'
import { extractErrorMessage } from '@/shared/api/error'
import { useAuthStore } from '@/stores/auth'

export function useAdminUserRoleAssignmentDrawer(options: {
  modelValue: Ref<boolean>
  user: Ref<AdminUserBasicResponse | null>
  closeDrawer: () => void
  onChanged: () => void
}) {
  const authStore = useAuthStore()
  const filters = reactive(createDefaultAdminUserRoleSearchFilters())
  const roleOptions = ref<SelectableAdminRoleOption[]>([])
  const selectedRoleIds = ref<string[]>([])
  const isLoading = ref(false)
  const isSubmitting = ref(false)

  const selectedCount = computed(() => selectedRoleIds.value.length)
  const userDisplay = computed(() => {
    const user = options.user.value

    if (!user) {
      return '-'
    }

    return user.displayName ? `${user.userName} / ${user.displayName}` : user.userName
  })

  async function loadAssignedRoles() {
    const user = options.user.value

    if (!user) {
      selectedRoleIds.value = []
      return
    }

    const result = await getAdminUserRoles(user.id)
    selectedRoleIds.value = resolveSelectedRoleIdsFromAssignments(result.roles)
  }

  async function loadSelectableRoles() {
    const result = await getSelectableAdminRoles(buildSelectableAdminRolesQuery(filters))
    roleOptions.value = mapSelectableAdminRolesToOptions(result.items)
  }

  async function loadData() {
    if (!options.modelValue.value || !options.user.value) {
      return
    }

    isLoading.value = true

    try {
      await Promise.all([loadAssignedRoles(), loadSelectableRoles()])
    } catch (error) {
      roleOptions.value = []
      selectedRoleIds.value = []
      ElMessage.error(extractErrorMessage(error, '加载管理员角色失败'))
    } finally {
      isLoading.value = false
    }
  }

  async function handleSearch() {
    isLoading.value = true

    try {
      await loadSelectableRoles()
    } catch (error) {
      roleOptions.value = []
      ElMessage.error(extractErrorMessage(error, '加载角色列表失败'))
    } finally {
      isLoading.value = false
    }
  }

  async function handleReset() {
    Object.assign(filters, createDefaultAdminUserRoleSearchFilters())
    await handleSearch()
  }

  async function handleSubmit() {
    const user = options.user.value

    if (!user) {
      return
    }

    isSubmitting.value = true

    try {
      await assignAdminUserRoles(user.id, buildAssignAdminUserRolesPayload(selectedRoleIds.value))

      if (user.id === authStore.userId) {
        try {
          await authStore.fetchCurrentAuthorization()
        } catch {
          ElMessage.warning('角色已保存，但当前权限同步失败，请刷新页面后重试')
        }
      }

      ElMessage.success('分配角色成功')
      options.closeDrawer()
      options.onChanged()
    } catch (error) {
      ElMessage.error(extractErrorMessage(error, '分配角色失败'))
    } finally {
      isSubmitting.value = false
    }
  }

  watch(
    () => [options.modelValue.value, options.user.value?.id] as const,
    () => {
      void loadData()
    },
    { immediate: true },
  )

  return {
    filters,
    handleReset,
    handleSearch,
    handleSubmit,
    isLoading,
    isSubmitting,
    roleOptions,
    selectedCount,
    selectedRoleIds,
    userDisplay,
  }
}
