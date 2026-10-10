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
| D10 | Initial EF migration is generated with `dotnet ef`, not hand-written | EF Core 9+ throws on model/snapshot drift; a hand-written snapshot is a risk with no verification possible. |
| D11 | M1 migration covers `Organizations` + `vector` extension only; other tables arrive with the milestone that needs them | Smaller, verifiable migrations; Users depend on Identity (M2). |
| D12 | `Guid.CreateVersion7()` for new IDs | Time-ordered UUIDs index better in PostgreSQL. |
| D13 | License not chosen | Owner's choice; README says so rather than assuming one. |
| D14 | Frontend uses the current Vite `react-ts` template tooling (oxlint, TypeScript 6) | Kept the template defaults; added a `typecheck` script. |
