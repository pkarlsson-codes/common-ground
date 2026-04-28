import { ResponseError } from '../api/runtime'

export async function getErrorMessage(error: unknown, fallback: string): Promise<string> {
  if (!(error instanceof ResponseError)) {
    return fallback
  }

  try {
    const problem = await error.response.clone().json() as {
      detail?: string
      title?: string
      errors?: Record<string, string[]>
    }

    if (problem.detail) return problem.detail

    const firstError = Object.values(problem.errors ?? {}).flat()[0]
    if (firstError) return firstError

    if (problem.title) return problem.title
  } catch {
    return fallback
  }

  return fallback
}
