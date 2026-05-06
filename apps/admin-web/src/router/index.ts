import { createRouter, createWebHistory } from 'vue-router'

import { registerAuthGuard } from '@/router/guards'
import { registerRouteLoadingHooks } from '@/router/loading'
import { routes } from '@/router/routes'

export const router = createRouter({
  history: createWebHistory(),
  routes,
})

registerAuthGuard(router)
registerRouteLoadingHooks(router)
