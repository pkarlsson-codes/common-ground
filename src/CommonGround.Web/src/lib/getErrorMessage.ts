import { ResponseError } from '../api/runtime'

export type ApiProblemDetails = {
  status?: number
  title?: string
  detail?: string
  message: string
  errors?: Record<string, string[]>
}

type ProblemPayload = {
  detail?: string
  title?: string
  errors?: Record<string, string[]>
}

const statusFallback = (status?: number): string | null => {
  if (!status) return null
  if (status === 404) return 'The requested resource was not found.'
  if (status === 409) return 'This action conflicts with the current application state.'
  if (status === 422) return 'One or more validation errors occurred. Review the fields and try again.'
  if (status >= 500) return 'Server error while processing the request. Please try again.'
  return null
}

export function getResponseStatus(error: unknown): number | undefined {
  return error instanceof ResponseError ? error.response.status : undefined
}

const parseProblemPayload = async (error: ResponseError): Promise<ProblemPayload | null> => {
  try {
    return await error.response.clone().json() as ProblemPayload
  } catch {
    return null
  }
}

export async function getApiProblem(error: unknown, fallback: string): Promise<ApiProblemDetails> {
  if (!(error instanceof ResponseError)) {
    return { message: fallback }
  }

  const status = error.response.status
  const payload = await parseProblemPayload(error)
  const firstFieldError = Object.values(payload?.errors ?? {}).flat()[0]

  const message = payload?.detail
    ?? firstFieldError
    ?? payload?.title
    ?? statusFallback(status)
    ?? fallback

  return {
    status,
    title: payload?.title,
    detail: payload?.detail,
    message,
    errors: payload?.errors,
  }
}

export async function getErrorMessage(error: unknown, fallback: string): Promise<string> {
  return (await getApiProblem(error, fallback)).message
}
