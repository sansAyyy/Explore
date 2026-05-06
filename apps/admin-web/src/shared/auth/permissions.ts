export function hasPagePermission(permissionCodes: string[], permissionCode?: string) {
  if (!permissionCode) {
    return true
  }

  return permissionCodes.includes(permissionCode)
}

export function hasButtonPermission(permissionCodes: string[], permissionCode: string) {
  return permissionCodes.includes(permissionCode)
}
