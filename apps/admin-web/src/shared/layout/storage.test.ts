import { beforeEach, describe, expect, it } from 'vitest'

import {
  clearPersistedAdminLayoutPreferences,
  createDefaultAdminLayoutPreferences,
  getPersistedAdminLayoutPreferences,
  persistAdminLayoutPreferences,
} from '@/shared/layout/storage'

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

describe('admin layout storage', () => {
  beforeEach(() => {
    Object.defineProperty(globalThis, 'localStorage', {
      value: new LocalStorageMock(),
      configurable: true,
      writable: true,
    })
  })

  it('returns null when no preferences are persisted', () => {
    expect(getPersistedAdminLayoutPreferences()).toBeNull()
  })

  it('persists and restores sidebar collapsed preference', () => {
    persistAdminLayoutPreferences({
      sidebarCollapsed: true,
    })

    expect(getPersistedAdminLayoutPreferences()).toEqual({
      sidebarCollapsed: true,
    })
  })

  it('falls back to defaults when persisted data is partial', () => {
    globalThis.localStorage.setItem('explore.admin.layout', JSON.stringify({}))

    expect(getPersistedAdminLayoutPreferences()).toEqual(createDefaultAdminLayoutPreferences())
  })

  it('clears broken persisted state', () => {
    globalThis.localStorage.setItem('explore.admin.layout', '{broken')

    expect(getPersistedAdminLayoutPreferences()).toBeNull()
    expect(globalThis.localStorage.getItem('explore.admin.layout')).toBeNull()
  })

  it('removes persisted preferences', () => {
    persistAdminLayoutPreferences({
      sidebarCollapsed: true,
    })

    clearPersistedAdminLayoutPreferences()

    expect(getPersistedAdminLayoutPreferences()).toBeNull()
  })
})
