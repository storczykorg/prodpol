# Prodpol — F# ASP.NET Core E-Commerce CMS

## Tech Stack
- **Backend**: .NET 10, F#, ASP.NET Core, Dapper + Npgsql, PostgreSQL 18
- **Identity**: ASP.NET Core Identity with custom `Employee` user store
- **Orchestration**: .NET Aspire (Postgres, PgAdmin, API, Frontend, Worker)
- **Frontend**: Vue 3, Vite 8, TypeScript 6, Tailwind CSS v4, DaisyUI v5
- **Async**: `AsyncResult<_, _>` from `Storczyk.Async` (not raw `Task`)
- **Testing**: NUnit + Moq (unit), TestHost (integration), Vitest + Playwright (frontend)
- **Formatter**: Fantomas 7 (`dotnet fantomas .`)

## Commands
| Command | What |
|---|---|
| `dotnet build` | Build entire .NET solution |
| `dotnet test` | Run all backend tests |
| `dotnet fantomas .` | Format all F# code |
| `dotnet run --project src/Storczyk.AppHost` | Launch full Aspire stack |
| `npm run dev -w src/Storczyk.Frontend` | Frontend dev server |

## Architecture
- **Repository pattern**: `IReadRepository`, `IRepository` interfaces → Dapper implementations in `lib/Storczyk.Prodpol.Data/Services/`
- **Controller pattern**: Controllers inherit `LoggedController` (maps `Async<'a>` → `Task<ActionResult>` with error handling)
- **Identity**: Custom `Employee` model → `PgEmployeeUserStore` (implements `IUserStore<Employee>` + `IUserPasswordStore<Employee>`) → `EmployeeSignInManager`
- **DI**: Auto-registration via `[<RegisterAsTransient>]` attribute + `RegisterFromRuntime()`

## F# Conventions
- **Async I/O**: Use `async { }` from `Storczyk.Async`, not `task { }` (except Identity interface implementations)
- **Domain models**: Mark with `[<Serializable; ProdpolModel>]` for Dapper type-map registration
- **Mutable entities** (`Employee`, `EmployeeRole`): Use `member val ... with get, set`
- **Read-only data** (`Product`, `EmployeePhoto`): Use records with `[<CLIMutable>]`
- **Error handling**: Exceptions (`NotFoundException`, `ValidationErrorException`) caught by `LoggedController`
- **SQL migrations**: All scripts in `lib/Storczyk.Database/Postgres/`, must be idempotent (`IF NOT EXISTS`, `CREATE OR REPLACE`)
- **Global.json**: `allowPrerelease: true`, `rollForward: latestMajor` — expect SDK updates
- **Pre-commit hook**: Runs `dotnet test` automatically

## Key Files
- `src/Storczyk.Prodpol/ProdpolServer.fs` — DI, middleware, Identity config
- `src/Storczyk.Prodpol/Utils/LoggedController.fs` — Base controller with error mapping
- `lib/Storczyk.Prodpol.Data/Services/Identity/` — All repository + Identity implementations
- `lib/Storczyk.Async/` — `AsyncResult`, `Task.wrap*`, custom exceptions
- `lib/Storczyk.Database/Postgres/` — SQL migration scripts (idempotent)
- `tests/Storczyk.Prodpol.Tests/` — Unit tests (NUnit)
- `tests/Storczyk.Prodpol.Integrations/` — Integration tests (TestHost)

## Detailed Reference
For comprehensive project documentation, read `GEMINI.md`.
