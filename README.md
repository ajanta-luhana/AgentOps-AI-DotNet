# AgentOps AI .NET

**Enterprise-Grade AI Agent Orchestration Platform Built with ASP.NET Core**

AgentOps AI .NET is a local-first AI agent platform built with ASP.NET Core, C#, PostgreSQL, Ollama and React.
Users submit a business goal; the platform plans with a local model, validates the plan, executes only approved
registered tools, persists every step, and returns a traceable report. No paid AI API is required.

> **Project status: Milestone 1 (scaffold) is complete and verified.** Most features below are the *target*, not what exists today.
> See [docs/status.md](docs/status.md) for exactly what has been built and verified.

## What exists today

- Clean Architecture solution skeleton (`Domain`, `Application`, `Infrastructure`, `Api`, `Worker`).
- API host with `/api/v1/health/live` and `/api/v1/health/ready`, Problem Details errors, startup-validated configuration, explicit CORS.
- Worker host skeleton (does **not** process tasks yet).
- React + TypeScript shell that shows real readiness data from the API.
- Docker Compose for PostgreSQL + pgvector, the `InitialCreate` EF Core migration (Organizations table + pgvector extension), GitHub Actions CI (first run on 2026-10-11 succeeded).

## Target features (planned milestones)

Registration/login with organizations and roles · agents and durable tasks with a validated state machine ·
Ollama structured planning · tool registry (`csv.profile`, `csv.analyze_sales`, `documents.search`, `reports.generate`) ·
deterministic CSV analytics · PDF/TXT RAG with pgvector and citations · approvals · evaluations · OpenTelemetry.

## Tech stack

ASP.NET Core / C# on .NET 10 · PostgreSQL + EF Core + Npgsql · pgvector · Ollama · React + TypeScript + Vite · xUnit.
Verified versions are listed in [docs/dependencies.md](docs/dependencies.md).

## Prerequisites

.NET 10 SDK, Git, Node.js LTS (22+), Docker Desktop, and Ollama (Ollama is not used until Milestone 4).

## Setup (Windows PowerShell)

```powershell
git clone https://github.com/ajanta-luhana/AgentOps-AI-DotNet.git
cd AgentOps-AI-DotNet

# 1. Database
Copy-Item .env.example .env          # edit POSTGRES_PASSWORD
docker compose up -d postgres

# 2. Backend configuration (secrets stay out of source control)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=127.0.0.1;Port=5432;Database=agentops;Username=agentops;Password=<POSTGRES_PASSWORD>" --project src/AgentOps.Api

# 3. Build and test
dotnet restore
dotnet build -c Release
dotnet test -c Release

# 4. Apply migrations (InitialCreate is committed)
dotnet tool install --global dotnet-ef --version 10.0.0
$env:ConnectionStrings__DefaultConnection = "Host=127.0.0.1;Port=5432;Database=agentops;Username=agentops;Password=<POSTGRES_PASSWORD>"
dotnet ef database update --project src/AgentOps.Infrastructure --startup-project src/AgentOps.Api

# 5. Run
dotnet run --project src/AgentOps.Api       # http://localhost:5080
dotnet run --project src/AgentOps.Worker    # separate terminal
cd frontend; npm install; npm run dev       # http://localhost:5173
```

Use `127.0.0.1` rather than `localhost` in the connection string (on Windows `localhost` can resolve to IPv6 first and slow the first connection).

Linux/macOS: the same commands work with `cp` instead of `Copy-Item` and `export ConnectionStrings__DefaultConnection=...`.

The worker reads the same user-secrets as the API (shared `UserSecretsId`).

## Configuration

| Setting | Where | Notes |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | user-secrets / env var | Required; startup fails without it |
| `AI:*` | `appsettings.json` placeholders | Set real model names via user-secrets before Milestone 4 |
| `Jwt:SigningKey` | user-secrets / env var (`Jwt__SigningKey`) | Required, at least 32 characters; startup fails without it (M2) |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:AccessTokenMinutes` | `appsettings.json` | Defaults are committed |
| `Cors:AllowedOrigins` | `appsettings.json` | Explicit origins only; `*` is rejected |
| `POSTGRES_PASSWORD` | `.env` (git-ignored) | See `.env.example` |

## Identity API (M2, branch `feature/m2-identity` — not yet verified)

```powershell
# one-time: a long random signing key (never commit it)
dotnet user-secrets set "Jwt:SigningKey" "<at-least-32-random-characters>" --project src/AgentOps.Api

# one-time: generate and apply the Users migration (not committed yet)
dotnet ef migrations add AddUsers --project src/AgentOps.Infrastructure --startup-project src/AgentOps.Api
dotnet ef database update --project src/AgentOps.Infrastructure --startup-project src/AgentOps.Api
```

| Endpoint | Auth | Purpose |
|---|---|---|
| `POST /api/v1/auth/register` | none | Body `{email, password (12-128 chars), organizationName}`. Creates an organization and its Admin user; returns a token |
| `POST /api/v1/auth/login` | none | Body `{email, password}`; returns `{accessToken, expiresAtUtc, user}` |
| `GET /api/v1/auth/me` | Bearer token | Returns the current user |

## Frontend commands

`npm run dev` · `npm run build` · `npm run lint` · `npm run typecheck` (all exist in `frontend/package.json`).

## Security model (current)

Secrets are never committed; configuration is validated at startup; errors are Problem Details without internal
details; health endpoints expose only status. Authentication, tenant isolation, rate limiting and the rest of the
threat model are **not implemented yet** — nothing here should be described as secure for real data.

## Test results

Milestone 1 (run 2026-10-10 on Windows, .NET SDK 10.0.401): 17 unit tests, 0 failed. No AI evaluation or
performance measurements exist yet. Details and what is still unverified: [docs/status.md](docs/status.md).

## Known issues and roadmap

See [docs/status.md](docs/status.md). Roadmap: M2 identity → M3 tasks/queue → M4 Ollama/tools → M5 CSV → M6 RAG →
M7 security/evaluation → M8 frontend → M9 delivery.

## License

Not yet chosen (see [docs/decisions.md](docs/decisions.md), D13).
