import { ApiError } from '@/api/generated'

interface ProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

export function getApiErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    const problem = error.body as ProblemDetails | undefined
    const validationMessages = problem?.errors
      ? Object.values(problem.errors).flat().filter(Boolean)
      : []

    if (validationMessages.length > 0) {
      return validationMessages.join(' ')
    }

    if (error.status === 409) {
      return problem?.detail ?? 'Запись уже изменилась. Обновите данные и повторите действие.'
    }

    return problem?.detail ?? problem?.title ?? `Ошибка API (${error.status})`
  }

  if (error instanceof Error) {
    return error.message
  }

  return 'Не удалось выполнить запрос.'
}
