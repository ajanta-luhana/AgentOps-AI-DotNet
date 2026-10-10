/** Shapes mirror the API contract (docs/architecture.md). Keep in sync with the backend DTOs. */
export interface ReadinessCheck {
  name: string
  status: 'Healthy' | 'Unhealthy'
}

export interface ReadinessResponse {
  status: 'Ready' | 'NotReady'
  checks: ReadinessCheck[]
}

/** RFC 9457 / 7807 Problem Details as returned by the API. */
export interface ProblemDetails {
  type?: string
  title: string
  status: number
  detail?: string
  traceId?: string
  errors?: Record<string, string[]>
}
