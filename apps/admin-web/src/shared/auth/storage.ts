import type { AdminAuthState } from '@/features/auth/types/auth'

const STORAGE_KEY = 'explore.admin.auth'

type StorageLike = Pick<Storage, 'getItem' | 'removeItem' | 'setItem'>

function getStorage(): StorageLike | null {
  if (typeof globalThis === 'undefined') {
    return null
  }

  return 'localStorage' in globalThis ? globalThis.localStorage : null
}

export function createEmptyAuthState(): AdminAuthState {
  return {
    accessToken: null,
    refreshToken: null,
    tokenType: null,
    expiresAt: null,
    refreshTokenExpiresAt: null,
    userId: null,
    userName: null,
    displayName: null,
    roleCodes: [],
    permissionCodes: [],
    pagePermissionCodes: [],
    buttonPermissionCodes: [],
  }
}

export function getPersistedAuthState() {
  const storage = getStorage()
  const rawValue = storage?.getItem(STORAGE_KEY)

  if (!rawValue) {
    return null
  }

  try {
    const parsedValue = JSON.parse(rawValue) as Partial<AdminAuthState>

    return {
      ...createEmptyAuthState(),
      ...parsedValue,
      roleCodes: parsedValue.roleCodes ?? [],
      permissionCodes: parsedValue.permissionCodes ?? [],
      pagePermissionCodes: parsedValue.pagePermissionCodes ?? [],
      buttonPermissionCodes: parsedValue.buttonPermissionCodes ?? [],
    }
  } catch {
    storage?.removeItem(STORAGE_KEY)
    return null
  }
}

export function persistAuthState(authState: AdminAuthState) {
  const storage = getStorage()
  storage?.setItem(STORAGE_KEY, JSON.stringify(authState))
}

export function clearPersistedAuthState() {
  const storage = getStorage()
  storage?.removeItem(STORAGE_KEY)
}
