import { describe, expect, it } from 'vitest'

import { formatDateTime } from '@/shared/utils/dateTime'

describe('dateTime utils', () => {
  it('returns fallback for empty values', () => {
    expect(formatDateTime(null)).toBe('-')
    expect(formatDateTime(undefined, { fallback: 'N/A' })).toBe('N/A')
  })

  it('returns fallback for invalid values', () => {
    expect(formatDateTime('invalid')).toBe('-')
    expect(formatDateTime('invalid', { fallback: 'No record' })).toBe('No record')
  })

  it('formats utc values in local time', () => {
    const value = '2026-01-01T08:30:00Z'
    const date = new Date(value)

    expect(formatDateTime(value)).toBe(
      `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(
        date.getMinutes(),
      )}`,
    )
  })
})

function pad(value: number) {
  return value.toString().padStart(2, '0')
}
