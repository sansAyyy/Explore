export function extractErrorMessage(error: unknown, fallbackMessage: string) {
  if (typeof error !== 'object' || error === null) {
    return fallbackMessage
  }

  const response = Reflect.get(error, 'response')

  if (typeof response === 'object' && response !== null) {
    const data = Reflect.get(response, 'data')

    if (typeof data === 'object' && data !== null) {
      const title = Reflect.get(data, 'title')

      if (typeof title === 'string' && title.trim().length > 0) {
        return title
      }
    }
  }

  const message = Reflect.get(error, 'message')

  return typeof message === 'string' && message.trim().length > 0 ? message : fallbackMessage
}
