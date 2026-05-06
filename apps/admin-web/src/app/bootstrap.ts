import '@/app/styles/global.css'
import 'element-plus/es/components/message/style/css'

import { createPinia } from 'pinia'
import { createApp } from 'vue'

import AppRoot from '@/app/AppRoot.vue'
import { router } from '@/router'
import { useAuthStore } from '@/stores/auth'
import { installHttpInterceptors } from '@/shared/api/http'

export async function bootstrapApp() {
  const app = createApp(AppRoot)
  const pinia = createPinia()

  app.use(pinia)
  installHttpInterceptors({ pinia, router })

  const authStore = useAuthStore(pinia)
  authStore.hydrateSession()

  app.use(router)
  app.mount('#app')

  void authStore.bootstrapSession()
}
