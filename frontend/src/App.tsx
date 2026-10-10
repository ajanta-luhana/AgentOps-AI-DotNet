import { useCallback, useEffect, useState } from 'react'
import { ApiError, getReadiness } from './api/client'
import type { ReadinessResponse } from './api/types'

export const PRODUCT_NAME = 'AgentOps AI .NET'
export const TAGLINE = 'Enterprise-Grade AI Agent Orchestration Platform Built with ASP.NET Core'

type LoadState =
  | { phase: 'loading' }
  | { phase: 'loaded'; data: ReadinessResponse }
  | { phase: 'error'; message: string }

export default function App() {
  const [state, setState] = useState<LoadState>({ phase: 'loading' })
  const [reloadKey, setReloadKey] = useState(0)

  const refresh = useCallback(() => {
    setState({ phase: 'loading' })
    setReloadKey((key) => key + 1)
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    getReadiness(controller.signal)
      .then((data) => setState({ phase: 'loaded', data }))
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') return
        const message = error instanceof ApiError ? error.message : 'Unexpected error.'
        setState({ phase: 'error', message })
      })
    return () => controller.abort()
  }, [reloadKey])

  return (
    <div className="shell">
      <header className="brand">
        <h1>{PRODUCT_NAME}</h1>
        <p className="tagline">{TAGLINE}</p>
      </header>

      <main>
        <section aria-labelledby="status-heading" className="card">
          <div className="card-header">
            <h2 id="status-heading">Service status</h2>
            <button type="button" onClick={refresh} disabled={state.phase === 'loading'}>
              {state.phase === 'loading' ? 'Checking…' : 'Refresh'}
            </button>
          </div>

          <div aria-live="polite">
            {state.phase === 'loading' && <p className="muted">Checking the API…</p>}
            {state.phase === 'error' && (
              <p role="alert" className="error">
                {state.message} Start the API (see README) and refresh.
              </p>
            )}
            {state.phase === 'loaded' && (
              <>
                <p>
                  Overall:{' '}
                  <strong className={state.data.status === 'Ready' ? 'ok' : 'error'}>{state.data.status}</strong>
                </p>
                <ul className="checks">
                  {state.data.checks.map((check) => (
                    <li key={check.name}>
                      <span>{check.name}</span>
                      <span className={check.status === 'Healthy' ? 'ok' : 'error'}>{check.status}</span>
                    </li>
                  ))}
                </ul>
              </>
            )}
          </div>
        </section>

        <p className="muted note">
          Milestone 1: scaffold only. Agents, tasks, documents and the knowledge assistant arrive in later milestones.
        </p>
      </main>
    </div>
  )
}
