import type { Component } from 'vue'
import type { RouteRecordRaw } from 'vue-router'

export interface AdminNavigationLinkItem {
  type: 'item'
  icon?: Component
  order: number
  pagePermission?: string
  path: string
  title: string
}

export interface AdminNavigationGroupItem {
  type: 'group'
  children: AdminNavigationLinkItem[]
  icon?: Component
  order: number
  title: string
}

export type AdminNavigationItem = AdminNavigationLinkItem | AdminNavigationGroupItem

export function isAdminNavigationGroup(item: AdminNavigationItem): item is AdminNavigationGroupItem {
  return item.type === 'group'
}

function getRouteMeta(route: RouteRecordRaw) {
  return route.meta ?? {}
}

function resolveChildPath(parentPath: string, childPath: string) {
  if (!childPath) {
    return parentPath || '/'
  }

  if (childPath.startsWith('/')) {
    return childPath
  }

  const normalizedParent = parentPath.endsWith('/') ? parentPath.slice(0, -1) : parentPath
  return `${normalizedParent}/${childPath}`.replace(/\/{2,}/g, '/')
}

function getMenuOrder(route: RouteRecordRaw) {
  return Number(route.meta?.menuOrder ?? Number.MAX_SAFE_INTEGER)
}

function sortRoutes(routes: readonly RouteRecordRaw[]) {
  return [...routes].sort((left, right) => getMenuOrder(left) - getMenuOrder(right))
}

function buildNavigationLink(route: RouteRecordRaw, fullPath: string): AdminNavigationLinkItem | null {
  const meta = getRouteMeta(route)

  if (meta.hideInMenu || meta.menuKind === 'group' || route.redirect || typeof meta.title !== 'string') {
    return null
  }

  return {
    type: 'item',
    icon: meta.menuIcon as Component | undefined,
    order: getMenuOrder(route),
    pagePermission: typeof meta.pagePermission === 'string' ? meta.pagePermission : undefined,
    path: fullPath,
    title: meta.title,
  }
}

function buildNavigationLinks(routes: readonly RouteRecordRaw[], parentPath: string): AdminNavigationLinkItem[] {
  const items: AdminNavigationLinkItem[] = []

  for (const route of sortRoutes(routes)) {
    const meta = getRouteMeta(route)
    const fullPath = resolveChildPath(parentPath, route.path)

    if (meta.menuKind === 'group') {
      items.push(...buildNavigationLinks(route.children ?? [], fullPath))
      continue
    }

    const link = buildNavigationLink(route, fullPath)
    if (link) {
      items.push(link)
      continue
    }

    if (route.children?.length) {
      items.push(...buildNavigationLinks(route.children, fullPath))
    }
  }

  return items.sort((left, right) => left.order - right.order)
}

function buildNavigationItems(routes: readonly RouteRecordRaw[], parentPath: string): AdminNavigationItem[] {
  const items: AdminNavigationItem[] = []

  for (const route of sortRoutes(routes)) {
    const meta = getRouteMeta(route)
    const fullPath = resolveChildPath(parentPath, route.path)

    if (meta.menuKind === 'group') {
      const children = buildNavigationLinks(route.children ?? [], fullPath)
      if (children.length === 0) {
        continue
      }

      items.push({
        type: 'group',
        children,
        icon: meta.menuIcon as Component | undefined,
        order: getMenuOrder(route),
        title: String(meta.title ?? ''),
      })
      continue
    }

    const link = buildNavigationLink(route, fullPath)
    if (link) {
      items.push(link)
    }
  }

  return items.sort((left, right) => left.order - right.order)
}

export function buildAdminNavigationFromRoutes(routes: readonly RouteRecordRaw[]): AdminNavigationItem[] {
  const shellRoute = routes.find((route) => route.path === '/' && route.children?.length)
  return buildNavigationItems(shellRoute?.children ?? [], '')
}

export function buildVisibleAdminNavigation(
  items: AdminNavigationItem[],
  canAccessPage: (permissionCode?: string) => boolean,
): AdminNavigationItem[] {
  return items.reduce<AdminNavigationItem[]>((result, item) => {
    if (isAdminNavigationGroup(item)) {
      const children = item.children.filter((child) => canAccessPage(child.pagePermission))

      if (children.length > 0) {
        result.push({
          ...item,
          children,
        })
      }

      return result
    }

    if (canAccessPage(item.pagePermission)) {
      result.push(item)
    }

    return result
  }, [])
}
