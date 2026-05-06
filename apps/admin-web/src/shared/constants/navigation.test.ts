import { describe, expect, it, vi } from 'vitest'

vi.mock('@/layouts/AdminLayout.vue', () => ({
  default: {},
}))

import { routes } from '@/router/routes'
import {
  buildAdminNavigationFromRoutes,
  buildVisibleAdminNavigation,
  isAdminNavigationGroup,
} from '@/shared/constants/navigation'

describe('navigation', () => {
  it('builds grouped navigation items from routes', () => {
    const navigation = buildAdminNavigationFromRoutes(routes)

    expect(navigation[0]).toEqual(
      expect.objectContaining({
        type: 'item',
        path: '/dashboard',
        pagePermission: 'dashboard.view',
      }),
    )

    const systemManagement = navigation.find(
      (item) => isAdminNavigationGroup(item) && item.title === '系统管理',
    )

    expect(systemManagement).toEqual(
      expect.objectContaining({
        children: expect.arrayContaining([
          expect.objectContaining({
            path: '/system/admin-users',
            pagePermission: 'admin_users.page',
          }),
          expect.objectContaining({
            path: '/system/admin-roles',
            pagePermission: 'admin_roles.page',
          }),
          expect.objectContaining({
            path: '/system/admin-permissions',
            pagePermission: 'admin_permissions.page',
          }),
        ]),
      }),
    )
  })

  it('hides empty groups after permission filtering', () => {
    const visibleNavigation = buildVisibleAdminNavigation(
      buildAdminNavigationFromRoutes(routes),
      (permissionCode) => permissionCode === 'dashboard.view' || permissionCode === 'customer_accounts.page',
    )

    expect(visibleNavigation).toHaveLength(2)
    expect(visibleNavigation[0]).toEqual(
      expect.objectContaining({
        type: 'item',
        path: '/dashboard',
      }),
    )

    const customerCenter = visibleNavigation[1]

    expect(isAdminNavigationGroup(customerCenter) && customerCenter.title).toBe('客户中心')
    if (!isAdminNavigationGroup(customerCenter)) {
      throw new Error('Expected customer center navigation group.')
    }

    expect(customerCenter.children).toEqual([
      expect.objectContaining({
        path: '/customer/customer-accounts',
        pagePermission: 'customer_accounts.page',
      }),
    ])
  })
})
