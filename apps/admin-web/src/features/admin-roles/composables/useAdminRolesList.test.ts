import { beforeEach, describe, expect, it, vi } from 'vitest'

import { useAdminRolesList } from '@/features/admin-roles/composables/useAdminRolesList'

const adminRolesApi = vi.hoisted(() => ({
  getPagedAdminRoles: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
}))

vi.mock('@/features/admin-roles/api/adminRolesApi', () => adminRolesApi)
vi.mock('element-plus', () => ({
  ElMessage: message,
}))

describe('useAdminRolesList', () => {
  beforeEach(() => {
    adminRolesApi.getPagedAdminRoles.mockReset()
    message.error.mockReset()
  })

  it('loads roles and updates result state', async () => {
    adminRolesApi.getPagedAdminRoles.mockResolvedValue({
      totalCount: 1,
      items: [
        {
          id: 'role-1',
          code: 'super_admin',
          name: '超级管理员',
          description: '系统超级管理员',
          isActive: true,
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: null,
        },
      ],
    })

    const list = useAdminRolesList()
    await list.loadRoles()

    expect(adminRolesApi.getPagedAdminRoles).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 10,
    })
    expect(list.roles.value).toHaveLength(1)
    expect(list.totalCount.value).toBe(1)
    expect(list.isLoading.value).toBe(false)
  })

  it('resets page index and reloads with current filters on search', async () => {
    adminRolesApi.getPagedAdminRoles.mockResolvedValue({
      totalCount: 0,
      items: [],
    })

    const list = useAdminRolesList()
    list.pagination.pageIndex = 3
    list.filters.keyword = '  super  '
    list.filters.status = 'enabled'

    await list.handleSearch()

    expect(list.pagination.pageIndex).toBe(1)
    expect(adminRolesApi.getPagedAdminRoles).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 10,
      keyword: 'super',
      isActive: true,
    })
  })

  it('restores default filters and pagination on reset', async () => {
    adminRolesApi.getPagedAdminRoles.mockResolvedValue({
      totalCount: 0,
      items: [],
    })

    const list = useAdminRolesList()
    list.filters.keyword = 'legacy'
    list.filters.status = 'disabled'
    list.pagination.pageIndex = 4
    list.pagination.pageSize = 50

    await list.handleReset()

    expect(list.filters).toMatchObject({
      keyword: '',
      status: 'all',
    })
    expect(list.pagination).toMatchObject({
      pageIndex: 1,
      pageSize: 10,
    })
    expect(adminRolesApi.getPagedAdminRoles).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 10,
    })
  })

  it('clears stale data and shows an error message when loading fails', async () => {
    adminRolesApi.getPagedAdminRoles.mockRejectedValue(new Error('request failed'))

    const list = useAdminRolesList()
    list.roles.value = [
      {
        id: 'role-1',
        code: 'stale_role',
        name: 'Stale Role',
        description: null,
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
      },
    ]
    list.totalCount.value = 1

    await list.loadRoles()

    expect(list.roles.value).toEqual([])
    expect(list.totalCount.value).toBe(0)
    expect(message.error).toHaveBeenCalledWith('request failed')
  })
})
