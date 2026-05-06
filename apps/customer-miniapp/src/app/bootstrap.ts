import { createSSRApp } from 'vue'

import App from '@/App.vue'

export function bootstrapApp() {
  const app = createSSRApp(App)

  return {
    app,
  }
}
