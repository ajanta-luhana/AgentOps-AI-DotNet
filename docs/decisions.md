# Decisions

| # | Decision | Reason |
|---|---|---|
| D1 | Product name **AgentOps AI .NET**, repository `AgentOps-AI-DotNet`, tagline *Enterprise-Grade AI Agent Orchestration Platform Built with ASP.NET Core* | Requested rebrand. Applied to user-facing text and package/assembly metadata. |
| D2 | C# namespaces/project names stay `AgentOps.*` | Requirements say architecture and technical structure are unchanged; the repo layout in the spec uses these names. |
| D3 | Classic `.sln` instead of `.slnx` | .NET 10 defaults to `.slnx`; `.sln` has the widest tooling/CI support. |
| D4 | Central package management (`Directory.Packages.props`) | One place to pin versions, as the spec requires. |
| D5 | Infrastructure and Worker use `FrameworkReference Microsoft.AspNetCore.App` | Gives DI/Hosting/Configuration without extra packages; avoids the Worker SDK's implicit hosting package. |
| D6 | No connection string or model name in committed config; connection string required at startup | Spec: no secrets in source control; fail fast instead of defaulting to a credential. |
| D7 | `AI:ChatModel`/`EmbeddingModel` keep the spec's `REPLACE_WITH_*` placeholders; rejection of placeholders is deferred to M4 | M1 does not call the model, so blocking startup would only hurt. |
| D8 | CORS wildcard origins rejected at startup | Spec: narrow explicit CORS origins. |
| D9 | `/health/ready` returns only check names + status | Spec: health endpoints must not disclose internals. |
| D10 | Initial EF migration is generated with `dotnet ef`, not hand-written | EF Core 9+ throws on model/snapshot drift. Generated and applied successfully on the owner machine. |
| D11 | M1 migration covers `Organizations` + `vector` extension only; other tables arrive with the milestone that needs them | Smaller, verifiable migrations; Users depend on Identity (M2). |
| D12 | `Guid.CreateVersion7()` for new IDs | Time-ordered UUIDs index better in PostgreSQL. |
| D13 | License not chosen | Owner's choice; README says so rather than assuming one. |
| D14 | Frontend uses the current Vite `react-ts` template tooling (oxlint, TypeScript 6) | Kept the template defaults; added a `typecheck` script. |
| D15 | Connection strings use `127.0.0.1`, not `localhost` | On Windows the first connection via `localhost` exceeded the 3 s readiness timeout; `127.0.0.1` did not. |
| D16 | EF-generated migration files marked `generated_code` in `src/AgentOps.Infrastructure/.editorconfig` | Generated code does not follow the repo's style rules (IDE0161). Hand-written code remains strictly checked. |
| D17 | Analyzer `CA1707` disabled under `tests/` only | `Method_Scenario_Result` test names are conventional; production code stays strict. |
| D18 | `AllowedHosts` is `localhost` in base config; `appsettings.Development.json` also allows `127.0.0.1` | Keeps production host filtering strict while allowing local tooling. |
| D19 | Best-effort database warm-up at startup; readiness timeout 5 s (was 3 s) | First readiness request after API start returned 503 on two separate days. Warm-up is non-fatal and logs only the exception type. |
| D20 | Passwords hashed with built-in PBKDF2-SHA256 (`Rfc2898DeriveBytes.Pbkdf2`), format `v1.{iterations}.{salt}.{hash}` | No extra package; stored iteration count allows raising the cost later. |
| D21 | Access token only (30 min default), HS256 JWT; refresh tokens deferred | Smallest correct slice first. Signing key is a secret (`Jwt:SigningKey`), min 32 chars, startup fails without it. |
| D22 | Registration creates an Organization and its first `Admin` user together | Every user belongs to exactly one organization (tenant root). |
| D23 | M2 `Users` migration is **not** committed from the authoring sandbox | EF Core 9+ throws on model/snapshot drift (see D10); generate it with `dotnet ef` on a machine with the SDK. |

