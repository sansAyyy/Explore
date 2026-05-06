import type { RouteRecordRaw } from 'vue-router'

import { House, MessageBox, SetUp, User } from '@element-plus/icons-vue'

import AdminLayout from '@/layouts/AdminLayout.vue'

export const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/features/auth/pages/LoginPage.vue'),
    meta: {
      title: '登录',
      hideInMenu: true,
    },
  },
  {
    path: '/',
    component: AdminLayout,
    meta: {
      requiresAuth: true,
    },
    children: [
      {
        path: '',
        redirect: {
          name: 'dashboard',
        },
      },
      {
        path: 'dashboard',
        name: 'dashboard',
        component: () => import('@/features/dashboard/pages/DashboardPage.vue'),
        meta: {
          requiresAuth: true,
          title: '仪表盘',
          pagePermission: 'dashboard.view',
          menuKind: 'item',
          menuIcon: House,
          menuOrder: 10,
        },
      },
      {
        path: 'profile',
        name: 'profile',
        component: () => import('@/features/current-admin/pages/ProfilePage.vue'),
        meta: {
          requiresAuth: true,
          title: '个人中心',
          hideInMenu: true,
        },
      },
      {
        path: 'system',
        redirect: {
          name: 'admin-users',
        },
        meta: {
          requiresAuth: true,
          title: '系统管理',
          menuKind: 'group',
          menuIcon: SetUp,
          menuOrder: 20,
        },
        children: [
          {
            path: 'admin-users',
            name: 'admin-users',
            component: () => import('@/features/admin-users/pages/AdminUsersPage.vue'),
            meta: {
              requiresAuth: true,
              title: '用户管理',
              pagePermission: 'admin_users.page',
              menuKind: 'item',
              menuOrder: 10,
            },
          },
          {
            path: 'admin-roles',
            name: 'admin-roles',
            component: () => import('@/features/admin-roles/pages/AdminRolesPage.vue'),
            meta: {
              requiresAuth: true,
              title: '角色管理',
              pagePermission: 'admin_roles.page',
              menuKind: 'item',
              menuOrder: 20,
            },
          },
          {
            path: 'admin-roles/:roleId/permissions',
            name: 'admin-role-permissions',
            component: () => import('@/features/admin-roles/pages/AdminRolePermissionsPage.vue'),
            meta: {
              requiresAuth: true,
              title: '角色权限配置',
              pagePermission: 'admin_roles.page',
              hideInMenu: true,
              activeMenu: '/system/admin-roles',
            },
          },
          {
            path: 'admin-permissions',
            name: 'admin-permissions',
            component: () => import('@/features/admin-permissions/pages/AdminPermissionsPage.vue'),
            meta: {
              requiresAuth: true,
              title: '权限管理',
              pagePermission: 'admin_permissions.page',
              menuKind: 'item',
              menuOrder: 30,
            },
          },
        ],
      },
      {
        path: 'customer',
        redirect: {
          name: 'customer-accounts',
        },
        meta: {
          requiresAuth: true,
          title: '客户中心',
          menuKind: 'group',
          menuIcon: User,
          menuOrder: 30,
        },
        children: [
          {
            path: 'customer-accounts',
            name: 'customer-accounts',
            component: () => import('@/features/customer-accounts/pages/CustomerAccountsPage.vue'),
            meta: {
              requiresAuth: true,
              title: '客户管理',
              pagePermission: 'customer_accounts.page',
              menuKind: 'item',
              menuOrder: 10,
            },
          },
        ],
      },
      {
        path: 'message',
        redirect: {
          name: 'message-templates',
        },
        meta: {
          requiresAuth: true,
          title: '消息中心',
          menuKind: 'group',
          menuIcon: MessageBox,
          menuOrder: 40,
        },
        children: [
          {
            path: 'message-templates',
            name: 'message-templates',
            component: () => import('@/features/message-templates/pages/MessageTemplatesPage.vue'),
            meta: {
              requiresAuth: true,
              title: '消息模板',
              pagePermission: 'message_templates.page',
              menuKind: 'item',
              menuOrder: 10,
            },
          },
          {
            path: 'site-messages',
            name: 'site-messages',
            component: () => import('@/features/site-messages/pages/SiteMessagesPage.vue'),
            meta: {
              requiresAuth: true,
              title: '站内信',
              hideInMenu: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: '/403',
    name: 'forbidden',
    component: () => import('@/features/errors/pages/ForbiddenPage.vue'),
    meta: {
      title: '无权限',
      hideInMenu: true,
    },
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/',
  },
]
