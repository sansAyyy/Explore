import { MessageBox, SetUp, User } from '@element-plus/icons-vue'
import { describe, expect, it, vi } from 'vitest'

vi.mock('@/layouts/AdminLayout.vue', () => ({
  default: {},
}))

import { routes } from '@/router/routes'

describe('routes', () => {
  it('registers grouped routes with prefixed urls under the authenticated shell', () => {
    const shellRoute = routes.find((route) => route.path === '/')
    const systemRoute = shellRoute?.children?.find((route) => route.path === 'system')
    const customerRoute = shellRoute?.children?.find((route) => route.path === 'customer')
    const messageRoute = shellRoute?.children?.find((route) => route.path === 'message')
    const customerAccountsRoute = customerRoute?.children?.find(
      (route) => route.path === 'customer-accounts',
    )

    expect(systemRoute).toEqual(
      expect.objectContaining({
        meta: expect.objectContaining({
          requiresAuth: true,
          title: '系统管理',
          menuKind: 'group',
          menuIcon: SetUp,
          menuOrder: 20,
        }),
      }),
    )

    expect(customerRoute).toEqual(
      expect.objectContaining({
        meta: expect.objectContaining({
          title: '客户中心',
          menuKind: 'group',
          menuIcon: User,
          menuOrder: 30,
        }),
      }),
    )

    expect(messageRoute).toEqual(
      expect.objectContaining({
        meta: expect.objectContaining({
          title: '消息中心',
          menuKind: 'group',
          menuIcon: MessageBox,
          menuOrder: 40,
        }),
      }),
    )

    expect(customerAccountsRoute).toEqual(
      expect.objectContaining({
        name: 'customer-accounts',
        meta: expect.objectContaining({
          requiresAuth: true,
          pagePermission: 'customer_accounts.page',
          title: '客户管理',
          menuOrder: 10,
        }),
      }),
    )
  })

  it('keeps role permission configuration hidden from the menu', () => {
    const shellRoute = routes.find((route) => route.path === '/')
    const systemRoute = shellRoute?.children?.find((route) => route.path === 'system')
    const rolePermissionsRoute = systemRoute?.children?.find(
      (route) => route.name === 'admin-role-permissions',
    )

    expect(rolePermissionsRoute).toEqual(
      expect.objectContaining({
        path: 'admin-roles/:roleId/permissions',
        meta: expect.objectContaining({
          hideInMenu: true,
          activeMenu: '/system/admin-roles',
          pagePermission: 'admin_roles.page',
        }),
      }),
    )
  })

  it('registers a hidden profile route that only requires authentication', () => {
    const shellRoute = routes.find((route) => route.path === '/')
    const profileRoute = shellRoute?.children?.find((route) => route.name === 'profile')

    expect(profileRoute).toEqual(
      expect.objectContaining({
        path: 'profile',
        meta: expect.objectContaining({
          requiresAuth: true,
          title: '个人中心',
          hideInMenu: true,
        }),
      }),
    )

    expect(profileRoute?.meta?.pagePermission).toBeUndefined()
  })

  it('registers a hidden authenticated site messages route outside the left menu', () => {
    const shellRoute = routes.find((route) => route.path === '/')
    const messageRoute = shellRoute?.children?.find((route) => route.path === 'message')
    const siteMessagesRoute = messageRoute?.children?.find((route) => route.name === 'site-messages')

    expect(siteMessagesRoute).toEqual(
      expect.objectContaining({
        path: 'site-messages',
        meta: expect.objectContaining({
          requiresAuth: true,
          title: '站内信',
          hideInMenu: true,
        }),
      }),
    )

    expect(siteMessagesRoute?.meta?.pagePermission).toBeUndefined()
  })
})
