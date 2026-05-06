import type { Component } from 'vue'

import 'vue-router'

declare module 'vue-router' {
  interface RouteMeta {
    activeMenu?: string
    hideInMenu?: boolean
    menuIcon?: Component
    menuKind?: 'group' | 'item'
    menuOrder?: number
    pagePermission?: string
    requiresAuth?: boolean
    title?: string
  }
}

export {}
