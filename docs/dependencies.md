# Dependencies

"Verified" = restored and used in a command that was actually run.

## Toolchain

| Component | Version | Status |
|---|---|---|
| .NET SDK | 10.0.112 (Ubuntu package) | Verified (built Domain/Application, ran Api/Worker) |
| Node.js / npm | 22.22.2 / 10.9.7 | Verified |
| PostgreSQL (local check only) | 16.15 + pgvector 0.6.0 | Verified |
| Compose image `pgvector/pgvector:pg17` | — | **Not verified** (no Docker in sandbox) |
| Ollama | — | **Not installed / not verified** |

## NuGet (pinned in `Directory.Packages.props`) — all **unverified**, NuGet was unreachable

| Package | Version | Used by |
|---|---|---|
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.0 | Infrastructure |
| Microsoft.EntityFrameworkCore.Design | 10.0.0 | Infrastructure, Api (design-time only) |
| Microsoft.NET.Test.Sdk | 17.14.1 | tests |
| xunit | 2.9.3 | tests |
| xunit.runner.visualstudio | 3.1.4 | tests |

Licenses: to be confirmed from each package's nuspec after a successful restore (not claimed here).

## npm (`frontend/`, installed versions) — **verified**

react 19.3.0, react-dom 19.3.0, typescript 6.0.3, vite 8.3.4, @vitejs/plugin-react 6.1.2, oxlint 1.87.0,
@types/react 19.3.0, @types/react-dom 19.3.0, @types/node 24.19.2. `npm install` reported 0 vulnerabilities at install time.
