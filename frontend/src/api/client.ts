import type { ProblemDetails, ReadinessResponse } from './types'

const API_BASE = '/api/v1'

export class ApiError extends Error {
  readonly problem: ProblemDetails | null

  constructor(message: string, problem: ProblemDetails | null) {
    super(message)
    this.name = 'ApiError'
    this.problem = problem
  }
}

/**
 * Readiness answers 503 with a valid body when a dependency is down. That is data, not a transport error,
 * so it is returned to the caller; anything else unexpected becomes an ApiError.
 */
export async function getReadiness(signal?: AbortSignal): Promise<ReadinessResponse> {
  let response: Response
  try {
    response = await fetch(`${API_BASE}/health/ready`, { signal, headers: { Accept: 'application/json' } })
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') throw error
    throw new ApiError('The API could not be reached.', null)
  }

  if (response.ok || response.status === 503) {
    const body: unknown = await response.json().catch(() => null)
    if (isReadinessResponse(body)) return body
  }

  const problem = (await response.json().catch(() => null)) as ProblemDetails | null
  throw new ApiError(problem?.title ?? `Unexpected API response (${response.status}).`, problem)
}

function isReadinessResponse(value: unknown): value is ReadinessResponse {
  if (typeof value !== 'object' || value === null) return false
  const candidate = value as Partial<ReadinessResponse>
  return (
    (candidate.status === 'Ready' || candidate.status === 'NotReady') &&
    Array.isArray(candidate.checks)
  )
}
