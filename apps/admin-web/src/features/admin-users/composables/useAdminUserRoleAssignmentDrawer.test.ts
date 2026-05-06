import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { nextTick, ref } from 'vue'

import { useAdminUserRoleAssignmentDrawer } from '@/features/admin-users/composables/useAdminUserRoleAssignmentDrawer'
import { useAuthStore } from '@/stores/auth'

const adminUsersApi = vi.hoisted(() => ({
  assignAdminUserRoles: vi.fn(),
  getAdminUserRoles: vi.fn(),
  getSelectableAdminRoles: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
  success: vi.fn(),
  warning: vi.fn(),
}))

vi.mock('@/features/admin-users/api/adminUsersApi', () => adminUsersApi)
vi.mock('element-plus', () => ({
  ElMessage: message,
}))

async function flushPromises() {
  await Promise.resolve()
  await Promise.resolve()
  await nextTick()
}

describe('useAdminUserRoleAssignmentDrawer', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    adminUsersApi.assignAdminUserRoles.mockReset()
    adminUsersApi.getAdminUserRoles.mockReset()
    adminUsersApi.getSelectableAdminRoles.mockReset()
    message.error.mockReset()
    message.success.mockReset()
    message.warning.mockReset()
  })

  it('loads assigned roles and selectable options when the drawer opens', async () => {
    adminUsersApi.getAdminUserRoles.mockResolvedValue({
      adminUserId: 'user-1',
      roles: [
        {
          id: 'role-1',
          code: 'super_admin',
          name: '超级管理员',
          isActive: true,
        },
        {
          id: 'role-2',
          code: 'legacy_admin',
          name: '历史角色',
          isActive: false,
        },
      ],
    })
    adminUsersApi.getSelectableAdminRoles.mockResolvedValue({
      totalCount: 1,
      items: [
        {
          id: 'role-1',
          code: 'super_admin',
          name: '超级管理员',
          description: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: null,
        },
      ],
    })

    const drawer = useAdminUserRoleAssignmentDrawer({
      closeDrawer: vi.fn(),
      modelValue: ref(true),
      onChanged: vi.fn(),
      user: ref({
        id: 'user-1',
        userName: 'seed-admin',
        email: 'seed-admin@example.test',
        phoneNumber: null,
        displayName: 'Platform Admin',
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
        lastLoginAt: null,
      }),
    })

    await flushPromises()

    expect(adminUsersApi.getAdminUserRoles).toHaveBeenCalledWith('user-1')
    expect(adminUsersApi.getSelectableAdminRoles).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 100,
      isActive: true,
    })
    expect(drawer.selectedRoleIds.value).toEqual(['role-1'])
    expect(drawer.roleOptions.value).toEqual([
      {
        value: 'role-1',
        label: '超级管理员',
        code: 'super_admin',
      },
    ])
    expect(drawer.selectedCount.value).toBe(1)
  })

  it('resets filters and reloads selectable roles', async () => {
    adminUsersApi.getAdminUserRoles.mockResolvedValue({
      adminUserId: 'user-2',
      roles: [],
    })
    adminUsersApi.getSelectableAdminRoles.mockResolvedValue({
      totalCount: 0,
      items: [],
    })

    const drawer = useAdminUserRoleAssignmentDrawer({
      closeDrawer: vi.fn(),
      modelValue: ref(true),
      onChanged: vi.fn(),
      user: ref({
        id: 'user-2',
        userName: 'editor',
        email: 'editor@explore.local',
        phoneNumber: null,
        displayName: 'Editor',
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
        lastLoginAt: null,
      }),
    })

    await flushPromises()
    adminUsersApi.getSelectableAdminRoles.mockClear()

    drawer.filters.keyword = ' legacy '
    await drawer.handleReset()

    expect(drawer.filters.keyword).toBe('')
    expect(adminUsersApi.getSelectableAdminRoles).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 100,
      isActive: true,
    })
  })

  it('submits selected roles and refreshes auth state for the current user', async () => {
    adminUsersApi.assignAdminUserRoles.mockResolvedValue({
      adminUserId: 'user-1',
      roles: [],
    })

    const authStore = useAuthStore()
    authStore.userId = 'user-1'
    const fetchCurrentAuthorization = vi
      .spyOn(authStore, 'fetchCurrentAuthorization')
      .mockResolvedValue({
        userId: 'user-1',
        userName: 'seed-admin',
        displayName: 'Platform Admin',
        roleCodes: ['super_admin'],
        permissionCodes: [],
        pagePermissionCodes: [],
        buttonPermissionCodes: [],
      })

    const closeDrawer = vi.fn()
    const onChanged = vi.fn()
    const drawer = useAdminUserRoleAssignmentDrawer({
      closeDrawer,
      modelValue: ref(false),
      onChanged,
      user: ref({
        id: 'user-1',
        userName: 'seed-admin',
        email: 'seed-admin@example.test',
        phoneNumber: null,
        displayName: 'Platform Admin',
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
        lastLoginAt: null,
      }),
    })

    drawer.selectedRoleIds.value = ['role-1', 'role-2']
    await drawer.handleSubmit()

    expect(adminUsersApi.assignAdminUserRoles).toHaveBeenCalledWith('user-1', {
      roleIds: ['role-1', 'role-2'],
    })
    expect(fetchCurrentAuthorization).toHaveBeenCalledTimes(1)
    expect(closeDrawer).toHaveBeenCalledTimes(1)
    expect(onChanged).toHaveBeenCalledTimes(1)
    expect(message.success).toHaveBeenCalledWith('分配角色成功')
  })
})
