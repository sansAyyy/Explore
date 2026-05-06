import { describe, expect, it } from 'vitest'

import { resolveAuthNavigation } from '@/router/guards'

describe('resolveAuthNavigation', () => {
  it('redirects anonymous users to login', () => {
    const result = resolveAuthNavigation(
      {
        name: 'dashboard',
        fullPath: '/dashboard',
        meta: { requiresAuth: true, pagePermission: 'dashboard.view' },
      },
      {
        isAuthenticated: false,
        hasPagePermission: () => false,
      },
    )

    expect(result).toEqual({
      name: 'login',
      query: { redirect: '/dashboard' },
    })
  })

  it('redirects authenticated users without page permission to forbidden', () => {
    const result = resolveAuthNavigation(
      {
        name: 'dashboard',
        fullPath: '/dashboard',
        meta: { requiresAuth: true, pagePermission: 'dashboard.view' },
      },
      {
        isAuthenticated: true,
        hasPagePermission: () => false,
      },
    )

    expect(result).toEqual({ name: 'forbidden' })
  })

  it('allows authenticated users with page permission', () => {
    const result = resolveAuthNavigation(
      {
        name: 'dashboard',
        fullPath: '/dashboard',
        meta: { requiresAuth: true, pagePermission: 'dashboard.view' },
      },
      {
        isAuthenticated: true,
        hasPagePermission: () => true,
      },
    )

    expect(result).toBe(true)
  })

  it('allows authenticated users to access protected routes without page permission', () => {
    const result = resolveAuthNavigation(
      {
        name: 'profile',
        fullPath: '/profile',
        meta: { requiresAuth: true },
      },
      {
        isAuthenticated: true,
        hasPagePermission: () => false,
      },
    )

    expect(result).toBe(true)
  })
})
