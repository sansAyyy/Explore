import { getGatewayBaseUrl } from '@/shared/config/env'

function isAbsoluteUrl(value: string) {
  return /^https?:\/\//i.test(value)
}

export function resolveAvatarUrl(value: string | null | undefined) {
  const trimmed = value?.trim()

  if (!trimmed) {
    return ''
  }

  if (isAbsoluteUrl(trimmed)) {
    return trimmed
  }

  return `${getGatewayBaseUrl()}${trimmed.startsWith('/') ? trimmed : `/${trimmed}`}`
}
