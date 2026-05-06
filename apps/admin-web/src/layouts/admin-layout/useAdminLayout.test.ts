import { nextTick, ref } from 'vue'
import { beforeEach, describe, expect, it } from 'vitest'

import { useAdminLayout } from '@/layouts/admin-layout/useAdminLayout'

class LocalStorageMock {
  private store = new Map<string, string>()

  getItem(key: string) {
    return this.store.get(key) ?? null
  }

  removeItem(key: string) {
    this.store.delete(key)
  }

  setItem(key: string, value: string) {
    this.store.set(key, value)
  }
}

describe('useAdminLayout', () => {
  beforeEach(() => {
    Object.defineProperty(globalThis, 'localStorage', {
      value: new LocalStorageMock(),
      configurable: true,
      writable: true,
    })
  })

  it('restores persisted collapsed state', () => {
    globalThis.localStorage.setItem(
      'explore.admin.layout',
      JSON.stringify({
        sidebarCollapsed: true,
      }),
    )

    const layout = useAdminLayout()

    expect(layout.isSidebarCollapsed.value).toBe(true)
  })

  it('toggles collapsed state and persists preference', () => {
    const layout = useAdminLayout()

    layout.toggleSidebarCollapsed()

    expect(layout.isSidebarCollapsed.value).toBe(true)
    expect(globalThis.localStorage.getItem('explore.admin.layout')).toBe(
      JSON.stringify({
        sidebarCollapsed: true,
      }),
    )
  })

  it('opens and closes mobile sidebar', () => {
    const layout = useAdminLayout()

    layout.openMobileSidebar()
    expect(layout.isMobileSidebarOpen.value).toBe(true)

    layout.closeMobileSidebar()
    expect(layout.isMobileSidebarOpen.value).toBe(false)
  })

  it('closes mobile sidebar when route changes', async () => {
    const routePath = ref('/dashboard')
    const layout = useAdminLayout(routePath)

    layout.openMobileSidebar()
    expect(layout.isMobileSidebarOpen.value).toBe(true)

    routePath.value = '/admin-users'
    await nextTick()

    expect(layout.isMobileSidebarOpen.value).toBe(false)
  })
})
