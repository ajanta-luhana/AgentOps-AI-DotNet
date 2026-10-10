# AgentOps AI .NET — Implementation Status

Last updated: 2026-10-10. Everything below is stated as **verified** only if it was actually executed; the
authoring sandbox had no access to NuGet (`api.nuget.org` unresolvable), no Docker and no Ollama.

## Milestones

- [ ] **M1 — Scaffold** *(in progress — blocked on NuGet access, see below)*
- [ ] M2 — Identity
- [ ] M3 — Core tasks
- [ ] M4 — Local AI
- [ ] M5 — CSV vertical slice
- [ ] M6 — RAG
- [ ] M7 — Security / evaluation
- [ ] M8 — Frontend
- [ ] M9 — Delivery

## M1 exit gate: "Clean build; migration applies to fresh DB"

**Not met.** The two things the gate requires cannot be done without NuGet:

1. Full-solution build (Infrastructure needs `Npgsql.EntityFrameworkCore.PostgreSQL`; test projects need xUnit).
2. Generating and applying the EF Core migration (`dotnet-ef` and the EF packages come from NuGet).

### M1 checklist

| Item | State | Evidence |
|---|---|---|
| Branding: AgentOps AI .NET / `AgentOps-AI-DotNet` / tagline | Done | Applied to README, docs, csproj metadata (`Directory.Build.props`), frontend title/meta/UI |
| Solution + 5 src projects, dependency rules | Done | `dotnet sln list`; Domain/Application have no outward refs |
| `AgentOps.Domain` builds (Release, warnings-as-errors) | **Verified** | 0 warnings, 0 errors |
| `AgentOps.Application` builds (Release, warnings-as-errors) | **Verified** | 0 warnings, 0 errors |
| `AgentOps.Api` / `AgentOps.Worker` compile | **Verified against a stub Infrastructure only** | Built in a scratch copy where Infrastructure was replaced by a stub with the same `AddInfrastructure` signature. Real Infrastructure was **not** compiled |
| Startup validation fails on: missing connection string, `AI:MaxCallsPerTask=0`, wildcard CORS | **Verified** (stubbed Infrastructure for the first) | Process aborted with the expected exception messages |
| `/api/v1/health/live` → 200; `/health/ready` → 503 when a probe fails, 200 when healthy | **Verified** (stub probe) | Real HTTP calls with curl |
| Readiness leaks no exception text; log records exception type only | **Verified** (stub probe threw a message containing a fake secret; 0 occurrences in response and log) | |
| Errors are RFC 9457 Problem Details with `traceId` | **Verified** (404 path) | `application/problem+json` |
| CORS: only configured origin allowed | **Verified** | Allowed origin echoed; foreign origin got no header |
| Worker starts, logs honestly that processing is not implemented, exits 0 on SIGTERM | **Verified** (stub Infrastructure) | |
| PostgreSQL 16 + pgvector 0.6.0 can be enabled on a fresh DB | **Verified** (apt-installed PG, throwaway DB, `CREATE EXTENSION vector` + a distance query) | Does **not** verify the compose image `pgvector/pgvector:pg17` |
| Frontend `lint`, `typecheck`, `build` | **Verified** | oxlint 0 warnings/errors; `tsc -b` clean; Vite build succeeded |
| Behavior assumed by the unit tests (Organization, AiOptions) | **Smoke-checked** with a throwaway console harness (13/13 checks) | This is **not** an xUnit run |
| Unit tests (`AgentOps.Domain.Tests`, `AgentOps.Application.Tests`) | Written, **not executed** | xUnit not restorable here |
| `AgentOpsDbContext`, design-time factory, `AddInfrastructure`, readiness probe | Written, **not compiled** | Needs EF Core/Npgsql |
| EF Core migration `InitialCreate` | **Not created** | Needs `dotnet ef`; deliberately not hand-written (EF 9+ throws on snapshot/model drift) |
| `docker-compose.yml` | Written, **not run** | No Docker in sandbox |
| `.github/workflows/ci.yml` | Written, **never run** | Action versions are major-tag pins, not reviewed for current status |
| OpenAPI document | **Deferred to M2** | `Microsoft.AspNetCore.OpenApi` is a NuGet package |
| Integration tests (`Api.IntegrationTests`, `Infrastructure.Tests`) | Deferred | Need NuGet + a database; planned for M2/M3 |

## To finish M1 (needs a machine with NuGet access)

```powershell
cd AgentOps-AI-DotNet
Copy-Item .env.example .env   # then edit POSTGRES_PASSWORD
docker compose up -d postgres
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=agentops;Username=agentops;Password=<POSTGRES_PASSWORD>"
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet ef migrations add InitialCreate --project src/AgentOps.Infrastructure --startup-project src/AgentOps.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/AgentOps.Infrastructure --startup-project src/AgentOps.Api
```

Then record the real outputs here and tick M1. Package versions in `Directory.Packages.props` were chosen from
memory and are **unverified**; if restore reports a missing version, adjust it and note it in `dependencies.md`.

## Known limitations right now

- The worker performs no task processing (M3).
- No authentication, tenants, agents, tasks, documents or AI calls exist yet.
- The frontend is a status shell showing real readiness data from the API; there are no other screens.
- HTTPS redirection, rate limiting and OpenTelemetry are not configured yet (planned M2/M7/M9).
