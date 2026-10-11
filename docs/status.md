# AgentOps AI .NET — Implementation Status

Last updated: 2026-10-11.

Evidence rule: an item is "Verified" only if a command was actually run and its output seen. Items marked
*(owner machine)* were run by the repository owner on Windows (.NET SDK 10.0.401, Docker Desktop) and the
output was reviewed; the rest were run in the authoring sandbox.

## Milestones

- [x] **M1 — Scaffold** (verified 2026-10-10/11, see below)
- [ ] M2 — Identity
- [ ] M3 — Core tasks
- [ ] M4 — Local AI
- [ ] M5 — CSV vertical slice
- [ ] M6 — RAG
- [ ] M7 — Security / evaluation
- [ ] M8 — Frontend
- [ ] M9 — Delivery

## M1 exit gate: "Clean build; migration applies to fresh DB" — **met**

| Item | State | Evidence |
|---|---|---|
| Branding: AgentOps AI .NET / `AgentOps-AI-DotNet` / tagline | Done | README, docs, `Directory.Build.props`, frontend title/meta/header/favicon |
| `dotnet restore` of all pinned NuGet packages | **Verified** *(owner machine)* | Restore complete; pinned versions in `Directory.Packages.props` resolved |
| Full solution build, Release, warnings-as-errors (7 projects) | **Verified** *(owner machine)* | "Build succeeded" for Domain, Application, Infrastructure, Api, Worker and both test projects |
| Unit tests | **Verified** *(owner machine)* | `total: 17, failed: 0, succeeded: 17, skipped: 0` (xUnit VSTest adapter 3.1.4) |
| Docker Compose PostgreSQL + pgvector | **Verified** *(owner machine)* | `pgvector/pgvector:pg17` pulled, container reached `healthy`, port bound to `127.0.0.1` only |
| EF Core migration `InitialCreate` generated and applied to a fresh DB | **Verified** *(owner machine)* | `Applying migration '20261010171448_InitialCreate'` ... `Done.` |
| Schema in database | **Verified** *(owner machine)* | `\dt`: `Organizations`, `__EFMigrationsHistory`; `\dx`: `vector 0.8.7` |
| API against the real database | **Verified** *(owner machine)* | `GET /api/v1/health/ready` -> `200`, `{"status":"Ready","checks":[{"name":"postgresql","status":"Healthy"}]}` |
| Frontend shows live API/DB status | **Verified** *(owner machine)* | Browser at `localhost:5173` showed the branded page and the real readiness state (also showed `NotReady` when the DB was unavailable/slow) |
| Startup validation fails on missing connection string, `AI:MaxCallsPerTask=0`, wildcard CORS | Verified (sandbox, stubbed Infrastructure) | Process aborted with expected messages |
| Readiness leaks no exception text; Problem Details with `traceId`; CORS allow-list; worker clean shutdown | Verified (sandbox, stubbed Infrastructure/probe) | curl/log checks |
| Frontend `lint`, `typecheck`, `build` | Verified (sandbox) | oxlint 0/0, `tsc -b` clean, Vite build OK |

## Issues found while verifying (all fixed unless noted)

- Test method names with underscores failed analyzer `CA1707` -> disabled for `tests/` only (`tests/.editorconfig`).
- EF-generated migration failed `IDE0161` (file-scoped namespace) -> migrations marked `generated_code` (`src/AgentOps.Infrastructure/.editorconfig`).
- Connection string with `localhost` made the first readiness probe exceed the 3 s timeout on Windows. Using `127.0.0.1` fixed it. The IPv6-first resolution explanation is **likely but not proven**.
- **Open observation (second occurrence):** on 2026-10-11 the *first* readiness request after starting the API returned `503 NotReady`; two later requests returned `Ready`. This matches a cold-start effect on the first database connection (also seen on 2026-10-10 with `localhost`). Mitigation added after M1 (**not yet verified**): a startup warm-up (`DatabaseWarmupService`) and a 5 s readiness timeout. Verify by restarting the API and requesting `/health/ready` immediately.
- `AllowedHosts` is `localhost` in base config, so requests with host `127.0.0.1` get `400`. Development config now also allows `127.0.0.1`.

## Not done / deferred

- GitHub Actions CI: run #1 on commit `a9f397c` **succeeded** (jobs `backend` 35 s and `frontend` 13 s; test-results artifact uploaded; seen on the run summary page, job logs not reviewed). Annotations: actions pinned to major tags target the deprecated Node 20 runtime, and `ubuntu-latest` migrates to Ubuntu 26 on 2026-10-19 (runner now pinned to `ubuntu-24.04`). Action versions still need a review.
- OpenAPI document: deferred to M2.
- Integration tests (`Api.IntegrationTests`, `Infrastructure.Tests`): deferred to M2/M3.
- The worker performs no task processing (M3).
- No authentication, tenants, agents, tasks, documents or AI calls exist yet.
- HTTPS redirection, rate limiting and OpenTelemetry are not configured yet.
- Licence not chosen. This is **not** client-ready software; it is the verified foundation only.

## How to run locally (M1)

See README. Three terminals: database (`docker compose up -d --wait postgres`), API
(`dotnet run --project src/AgentOps.Api`), frontend (`cd frontend; npm run dev`), then open `http://localhost:5173`.
