import { beforeEach, describe, expect, it, vi } from 'vitest'

import { useAdminUsersList } from '@/features/admin-users/composables/useAdminUsersList'

const adminUsersApi = vi.hoisted(() => ({
  getPagedAdminUsers: vi.fn(),
}))

const message = vi.hoisted(() => ({
  error: vi.fn(),
}))

vi.mock('@/features/admin-users/api/adminUsersApi', () => adminUsersApi)
vi.mock('element-plus', () => ({
  ElMessage: message,
}))

describe('useAdminUsersList', () => {
  beforeEach(() => {
    adminUsersApi.getPagedAdminUsers.mockReset()
    message.error.mockReset()
  })

  it('loads users and updates result state', async () => {
    adminUsersApi.getPagedAdminUsers.mockResolvedValue({
      totalCount: 1,
      items: [
        {
          id: 'user-1',
          userName: 'seed-admin',
          email: 'seed-admin@example.test',
          phoneNumber: '13800138000',
          displayName: 'Platform Admin',
          isActive: true,
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: null,
          lastLoginAt: null,
        },
      ],
    })

    const list = useAdminUsersList()
    await list.loadUsers()

    expect(adminUsersApi.getPagedAdminUsers).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 10,
    })
    expect(list.users.value).toHaveLength(1)
    expect(list.totalCount.value).toBe(1)
    expect(list.isLoading.value).toBe(false)
  })

  it('resets page index and reloads with current filters on search', async () => {
    adminUsersApi.getPagedAdminUsers.mockResolvedValue({
      totalCount: 0,
      items: [],
    })

    const list = useAdminUsersList()
    list.pagination.pageIndex = 3
    list.filters.keyword = '  seed-admin '
    list.filters.status = 'enabled'

    await list.handleSearch()

    expect(list.pagination.pageIndex).toBe(1)
    expect(adminUsersApi.getPagedAdminUsers).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 10,
      keyword: 'seed-admin',
      isActive: true,
    })
  })

  it('restores default filters and pagination on reset', async () => {
    adminUsersApi.getPagedAdminUsers.mockResolvedValue({
      totalCount: 0,
      items: [],
    })

    const list = useAdminUsersList()
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
    expect(adminUsersApi.getPagedAdminUsers).toHaveBeenCalledWith({
      pageIndex: 1,
      pageSize: 10,
    })
  })

  it('clears stale data and shows an error message when loading fails', async () => {
    adminUsersApi.getPagedAdminUsers.mockRejectedValue(new Error('request failed'))

    const list = useAdminUsersList()
    list.users.value = [
      {
        id: 'user-1',
        userName: 'stale',
        email: 'stale@explore.local',
        phoneNumber: null,
        displayName: 'Stale User',
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
        lastLoginAt: null,
      },
    ]
    list.totalCount.value = 1

    await list.loadUsers()

    expect(list.users.value).toEqual([])
    expect(list.totalCount.value).toBe(0)
    expect(message.error).toHaveBeenCalledWith('request failed')
  })
})
