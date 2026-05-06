import { describe, expect, it, vi, afterEach } from 'vitest'

import {
  registerRouteLoadingHooks,
  resetRouteLoadingStateForTest,
  useRouteLoading,
} from '@/router/loading'

afterEach(() => {
  resetRouteLoadingStateForTest()
})

describe('route loading hooks', () => {
  it('toggles loading state around navigation', async () => {
    const handlers: {
      afterEach: null | (() => void)
      beforeEach: null | ((to: { fullPath: string }, from: { fullPath: string }) => void)
      onError: null | ((error: unknown) => void)
    } = {
      afterEach: null,
      beforeEach: null,
      onError: null,
    }

    registerRouteLoadingHooks({
      beforeEach(handler: (to: { fullPath: string }, from: { fullPath: string }) => void) {
        handlers.beforeEach = handler
      },
      afterEach(handler: () => void) {
        handlers.afterEach = handler
      },
      onError(handler: (error: unknown) => void) {
        handlers.onError = handler
      },
    } as never)

    const { isRouteNavigating } = useRouteLoading()

    expect(isRouteNavigating.value).toBe(false)

    if (!handlers.beforeEach || !handlers.afterEach || !handlers.onError) {
      throw new Error('Expected route loading hooks to register handlers.')
    }

    handlers.beforeEach({ fullPath: '/system/admin-users' }, { fullPath: '/dashboard' })
    expect(isRouteNavigating.value).toBe(true)

    handlers.afterEach()
    expect(isRouteNavigating.value).toBe(false)

    handlers.beforeEach({ fullPath: '/system/admin-roles' }, { fullPath: '/dashboard' })
    expect(isRouteNavigating.value).toBe(true)

    handlers.onError(new Error('chunk load failed'))
    expect(isRouteNavigating.value).toBe(false)
  })

  it('keeps loading state idle when navigating to the same path and avoids duplicate hook registration', () => {
    const beforeEach = vi.fn()
    const afterEach = vi.fn()
    const onError = vi.fn()
    const routerMock = {
      beforeEach,
      afterEach,
      onError,
    }

    registerRouteLoadingHooks(routerMock as never)
    registerRouteLoadingHooks(routerMock as never)

    expect(beforeEach).toHaveBeenCalledTimes(1)
    expect(afterEach).toHaveBeenCalledTimes(1)
    expect(onError).toHaveBeenCalledTimes(1)

    const beforeEachHandler = beforeEach.mock.calls[0][0] as (
      to: { fullPath: string },
      from: { fullPath: string },
    ) => void

    beforeEachHandler({ fullPath: '/dashboard' }, { fullPath: '/dashboard' })

    const { isRouteNavigating } = useRouteLoading()
    expect(isRouteNavigating.value).toBe(false)
  })
})
