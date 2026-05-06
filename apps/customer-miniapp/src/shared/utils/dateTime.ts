export interface FormatDateTimeOptions {
  fallback?: string
}

export function formatDateTime(
  value: string | null | undefined,
  options: FormatDateTimeOptions = {},
) {
  const { fallback = '-' } = options

  if (!value) {
    return fallback
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return fallback
  }

  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(
    date.getMinutes(),
  )}`
}

function pad(value: number) {
  return value.toString().padStart(2, '0')
}
