import type { Router } from 'vue-router'

import { useAuthStore } from '@/stores/auth'

interface GuardRouteLike {
  fullPath: string
  meta: {
    pagePermission?: string
    requiresAuth?: boolean
  }
  name?: string | symbol | null
}

export interface AuthStoreLike {
  isAuthenticated: boolean
  hasPagePermission: (code?: string) => boolean
}

export type GuardResult =
  | true
  | {
      name: 'dashboard' | 'forbidden' | 'login'
      query?: Record<string, string>
    }

export function resolveAuthNavigation(to: GuardRouteLike, authStore: AuthStoreLike): GuardResult {
  if (to.name === 'login' && authStore.isAuthenticated) {
    return { name: 'dashboard' }
  }

  if (!to.meta.requiresAuth) {
    return true
  }

  if (!authStore.isAuthenticated) {
    return {
      name: 'login',
      query: { redirect: to.fullPath },
    }
  }

  if (to.meta.pagePermission && !authStore.hasPagePermission(to.meta.pagePermission)) {
    return { name: 'forbidden' }
  }

  return true
}

export function registerAuthGuard(router: Router) {
  router.beforeEach((to) => {
    const authStore = useAuthStore()
    return resolveAuthNavigation(to, authStore)
  })
}
