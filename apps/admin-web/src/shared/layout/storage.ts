export interface AdminLayoutPreferences {
  sidebarCollapsed: boolean
}

const STORAGE_KEY = 'explore.admin.layout'

type StorageLike = Pick<Storage, 'getItem' | 'removeItem' | 'setItem'>

function getStorage(): StorageLike | null {
  if (typeof globalThis === 'undefined') {
    return null
  }

  return 'localStorage' in globalThis ? globalThis.localStorage : null
}

export function createDefaultAdminLayoutPreferences(): AdminLayoutPreferences {
  return {
    sidebarCollapsed: false,
  }
}

export function getPersistedAdminLayoutPreferences() {
  const storage = getStorage()
  const rawValue = storage?.getItem(STORAGE_KEY)

  if (!rawValue) {
    return null
  }

  try {
    const parsedValue = JSON.parse(rawValue) as Partial<AdminLayoutPreferences>

    return {
      ...createDefaultAdminLayoutPreferences(),
      ...parsedValue,
      sidebarCollapsed: parsedValue.sidebarCollapsed ?? false,
    }
  } catch {
    storage?.removeItem(STORAGE_KEY)
    return null
  }
}

export function persistAdminLayoutPreferences(preferences: AdminLayoutPreferences) {
  const storage = getStorage()
  storage?.setItem(STORAGE_KEY, JSON.stringify(preferences))
}

export function clearPersistedAdminLayoutPreferences() {
  const storage = getStorage()
  storage?.removeItem(STORAGE_KEY)
}
