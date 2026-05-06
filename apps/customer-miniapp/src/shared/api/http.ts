import { getGatewayBaseUrl } from '@/shared/config/env'

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE'
type HttpRequestBody = string | object | ArrayBuffer

export interface ApiProblemDetails {
  title?: string
  detail?: string
  status?: number
  errorCode?: string
  errors?: Record<string, string[]>
}

export interface HttpRequestOptions<TBody extends HttpRequestBody = object> {
  url: string
  method?: HttpMethod
  data?: TBody
  headers?: Record<string, string>
  skipAuthToken?: boolean
  skipAuthRefresh?: boolean
}

interface HttpAuthHandlers {
  getAccessToken: () => string | null
  hasRefreshToken: () => boolean
  refreshSession: () => Promise<boolean>
  clearSession: () => void
}

export class ApiError extends Error {
  readonly status: number
  readonly errorCode?: string
  readonly details?: ApiProblemDetails

  constructor(message: string, status: number, errorCode?: string, details?: ApiProblemDetails) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.errorCode = errorCode
    this.details = details
  }
}

let authHandlers: HttpAuthHandlers = {
  getAccessToken: () => null,
  hasRefreshToken: () => false,
  refreshSession: async () => false,
  clearSession: () => undefined,
}

export function configureHttpAuth(handlers: Partial<HttpAuthHandlers>) {
  authHandlers = {
    ...authHandlers,
    ...handlers,
  }
}

export function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError
}

export async function httpRequest<TResponse, TBody extends HttpRequestBody = object>(
  options: HttpRequestOptions<TBody>,
  hasRetried = false,
): Promise<TResponse> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...options.headers,
  }

  if (!options.skipAuthToken) {
    const accessToken = authHandlers.getAccessToken()

    if (accessToken) {
      headers.Authorization = `Bearer ${accessToken}`
    }
  }

  let response: UniApp.RequestSuccessCallbackResult

  try {
    response = await rawRequest({
      url: resolveRequestUrl(options.url),
      method: options.method ?? 'GET',
      data: options.data,
      header: headers,
    })
  } catch (error) {
    throw new ApiError(getNetworkErrorMessage(error), 0)
  }

  if (isSuccessfulStatus(response.statusCode)) {
    return normalizeSuccessData<TResponse>(response)
  }

  const apiError = createApiError(response)

  if (
    response.statusCode === 401 &&
    !options.skipAuthRefresh &&
    !hasRetried &&
    authHandlers.hasRefreshToken()
  ) {
    try {
      const refreshed = await authHandlers.refreshSession()

      if (refreshed) {
        return httpRequest<TResponse, TBody>(options, true)
      }
    } catch {
      authHandlers.clearSession()
    }
  }

  throw apiError
}

function rawRequest(options: UniApp.RequestOptions) {
  return new Promise<UniApp.RequestSuccessCallbackResult>((resolve, reject) => {
    uni.request({
      ...options,
      success: resolve,
      fail: reject,
    })
  })
}

function resolveRequestUrl(path: string) {
  if (/^https?:\/\//.test(path)) {
    return path
  }

  return `${getGatewayBaseUrl()}${path.startsWith('/') ? path : `/${path}`}`
}

function isSuccessfulStatus(statusCode: number) {
  return statusCode >= 200 && statusCode < 300
}

function normalizeSuccessData<TResponse>(response: UniApp.RequestSuccessCallbackResult) {
  if (response.statusCode === 204 || typeof response.data === 'undefined' || response.data === '') {
    return undefined as TResponse
  }

  return response.data as TResponse
}

function createApiError(response: UniApp.RequestSuccessCallbackResult) {
  const details = parseProblemDetails(response.data, response.statusCode)
  const message = getProblemMessage(details) ?? `请求失败 (${response.statusCode})`

  return new ApiError(message, response.statusCode, details.errorCode, details)
}

function parseProblemDetails(data: unknown, status: number): ApiProblemDetails {
  if (!data || typeof data !== 'object' || Array.isArray(data)) {
    return {
      status,
      title: typeof data === 'string' ? data : undefined,
    }
  }

  const candidate = data as Record<string, unknown>

  return {
    status: typeof candidate.status === 'number' ? candidate.status : status,
    title: typeof candidate.title === 'string' ? candidate.title : undefined,
    detail: typeof candidate.detail === 'string' ? candidate.detail : undefined,
    errorCode: typeof candidate.errorCode === 'string' ? candidate.errorCode : undefined,
    errors: normalizeValidationErrors(candidate.errors),
  }
}

function normalizeValidationErrors(value: unknown) {
  if (!value || typeof value !== 'object' || Array.isArray(value)) {
    return undefined
  }

  const entries = Object.entries(value as Record<string, unknown>).reduce<Record<string, string[]>>(
    (result, [key, item]) => {
      if (Array.isArray(item)) {
        const messages = item.filter((message): message is string => typeof message === 'string')

        if (messages.length > 0) {
          result[key] = messages
        }
      }

      return result
    },
    {},
  )

  return Object.keys(entries).length > 0 ? entries : undefined
}

function getProblemMessage(details: ApiProblemDetails) {
  if (details.errors) {
    const messages = Object.values(details.errors).flat()

    if (messages.length > 0) {
      return messages[0]
    }
  }

  return details.title || details.detail
}

function getNetworkErrorMessage(error: unknown) {
  if (error instanceof Error && error.message) {
    return error.message
  }

  return '网络请求失败，请稍后重试'
}
