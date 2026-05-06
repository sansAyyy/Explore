import type { Router } from 'vue-router'
import { readonly, ref } from 'vue'

const isRouteNavigatingState = ref(false)
let hasInstalledRouteLoadingHooks = false

export function registerRouteLoadingHooks(router: Router) {
  if (hasInstalledRouteLoadingHooks) {
    return
  }

  hasInstalledRouteLoadingHooks = true

  router.beforeEach((to, from) => {
    isRouteNavigatingState.value = to.fullPath !== from.fullPath
  })

  router.afterEach(() => {
    isRouteNavigatingState.value = false
  })

  router.onError(() => {
    isRouteNavigatingState.value = false
  })
}

export function useRouteLoading() {
  return {
    isRouteNavigating: readonly(isRouteNavigatingState),
  }
}

export function resetRouteLoadingStateForTest() {
  isRouteNavigatingState.value = false
  hasInstalledRouteLoadingHooks = false
}
