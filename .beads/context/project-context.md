# Project Context (Maintained by Orchestrator)

## Tooling
- Backend: .NET 9 / ASP.NET Core 9
- ORM: EF Core 9 Code First (SQL Server)
- Test runner: nUnit + Moq (CRM.Tests)
- Frontend package manager: npm
- Frontend test runner: Vitest + React Testing Library (src/)
- Frontend build: Vite
- Frontend linter: TypeScript strict mode

## Solution Layout
```
CRM.sln
CRM.Domain/           — Entities only, no deps
CRM.Application/      — CQRS, MediatR, FluentValidation, interfaces
CRM.Infrastructure/   — EF Core, repositories, JWT service
CRM.API/              — Controllers, Program.cs, middleware
CRM.Tests/            — nUnit tests
src/                  — React + TypeScript frontend
```

## Key Patterns
- CQRS: each feature = Command/Query record + Handler in CRM.Application/Features/{Domain}/{Action}/
- Interfaces: IJwtService, IUserRepository defined in CRM.Application/Interfaces/
- DI registration: all services registered in Program.cs (or extension methods called from it)
- JWT secret: dotnet user-secrets for local dev; never in appsettings.json as real value
- Axios: src/api/client.ts — base instance + Bearer interceptor

## Completed Work Units
| WU | Title | Key Files | Services Created |
|----|-------|-----------|-----------------|
| — | — | — | — |

## Established Patterns
(populated after first WU commits)
