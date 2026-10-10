# Architecture — AgentOps AI .NET

*Enterprise-Grade AI Agent Orchestration Platform Built with ASP.NET Core*

Modular monolith following Clean Architecture. Only what exists in the current milestone is marked implemented.

```
frontend (React + TS) ──HTTP──▶ AgentOps.Api ──▶ AgentOps.Application ──▶ AgentOps.Domain
                                     │                    ▲
                                     ▼                    │
                              AgentOps.Infrastructure ────┘   (EF Core/Npgsql, Ollama, pgvector, files)
                                     ▲
                              AgentOps.Worker  (durable queue, leases, execution — M3+)
```

## Dependency rules (enforced by project references)

- **Domain**: no dependencies.
- **Application**: references Domain only. Defines abstractions such as `IReadinessProbe` and typed options such as `AiOptions`.
- **Infrastructure**: references Application. Implements persistence and (later) Ollama/pgvector/file adapters.
- **Api / Worker**: composition roots; reference Application + Infrastructure.
- **Frontend**: talks to the API only.

## Implemented in M1

- Health: `GET /api/v1/health/live`, `GET /api/v1/health/ready` (dependency status only).
- Problem Details error responses with `traceId`.
- Startup-validated typed configuration; explicit CORS origins.
- Domain root entity `Organization`.

## Not implemented yet

Authentication, tenancy enforcement, agents, tasks, queue, tools, Ollama client, CSV analytics, RAG,
approvals, evaluations, telemetry, OpenAPI. See `docs/status.md`.
