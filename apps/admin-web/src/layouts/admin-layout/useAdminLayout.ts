import { ref, watch, type Ref } from 'vue'

import {
  createDefaultAdminLayoutPreferences,
  getPersistedAdminLayoutPreferences,
  persistAdminLayoutPreferences,
} from '@/shared/layout/storage'

export function useAdminLayout(routePath?: Ref<string>) {
  const persistedPreferences =
    getPersistedAdminLayoutPreferences() ?? createDefaultAdminLayoutPreferences()

  const isSidebarCollapsed = ref(persistedPreferences.sidebarCollapsed)
  const isMobileSidebarOpen = ref(false)

  function persistSidebarCollapsed(value: boolean) {
    persistAdminLayoutPreferences({
      sidebarCollapsed: value,
    })
  }

  function setSidebarCollapsed(value: boolean) {
    isSidebarCollapsed.value = value
    persistSidebarCollapsed(value)
  }

  function toggleSidebarCollapsed() {
    setSidebarCollapsed(!isSidebarCollapsed.value)
  }

  function openMobileSidebar() {
    isMobileSidebarOpen.value = true
  }

  function closeMobileSidebar() {
    isMobileSidebarOpen.value = false
  }

  if (routePath) {
    watch(routePath, () => {
      closeMobileSidebar()
    })
  }

  return {
    closeMobileSidebar,
    isMobileSidebarOpen,
    isSidebarCollapsed,
    openMobileSidebar,
    setSidebarCollapsed,
    toggleSidebarCollapsed,
  }
}
