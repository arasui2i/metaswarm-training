---
name: crm-architecture
description: Core architecture decisions and layer boundaries for the CRM project
metadata:
  type: architecture
---

# CRM Architecture

## Solution Structure

```
CRM.Domain/          — Entities, value objects, domain interfaces (no dependencies)
CRM.Application/     — CQRS commands/queries, MediatR handlers, FluentValidation validators
CRM.Infrastructure/  — EF Core DbContext, migrations, repositories, JWT service
CRM.API/             — ASP.NET Core controllers, middleware, DI wiring (Program.cs)
src/                 — React + TypeScript frontend
```

## Backend Patterns

- **Clean Architecture**: dependencies point inward (Domain ← Application ← Infrastructure ← API)
- **CQRS via MediatR**: each feature gets a `Command` or `Query` record + `Handler` in `CRM.Application/Features/`
- **Repository pattern**: `IUserRepository`, etc. defined in Application, implemented in Infrastructure
- **FluentValidation**: validators registered as `AbstractValidator<TCommand>`, run via MediatR pipeline behavior
- **JWT**: `IJwtService` / `JwtService` — token claims include userId, email, roles[]
- **Role-Permission RBAC**: `PermissionAuthorizationHandler` maps named policies (e.g. `customers.view`) to JWT claims

## Frontend Patterns

- **Vite source root**: `src/src/` (nested). npm project is `src/`, Vite source root is `src/src/`. All imports are relative to `src/src/`.
- **Auth Context** (`src/src/context/AuthContext.tsx`): stores `{ user, token, isAuthenticated }`, rehydrates from storage on mount via `parseJwt()`
- **Protected Route** (`src/src/components/ProtectedRoute.tsx`): `isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />`
- **React Query**: all server state via `useQuery` / `useMutation` hooks in `src/src/hooks/`
- **API layer** (`src/src/api/`): `client.ts` (Axios instance + Bearer interceptor), one file per domain (auth.ts, leads.ts, accounts.ts)
- **Token storage**: `localStorage` when RememberMe=true, `sessionStorage` when RememberMe=false
- **MUI version**: v9 — use `slotProps={{ input: { endAdornment: ... } }}` NOT deprecated `InputProps`
- **Router**: React Router DOM v7 — same API as v6 for basic routing

## Feature Folder Convention

Backend:
```
CRM.Application/Features/{Domain}/{Action}/
  {Action}Command.cs        — record with properties
  {Action}CommandHandler.cs — implements IRequestHandler
  {Action}CommandValidator.cs — FluentValidation
```

Frontend:
```
src/pages/{Feature}/
  {Feature}ListPage.tsx
  {Feature}FormPage.tsx
src/hooks/use{Feature}.ts
src/api/{feature}.ts
```

## Database

- EF Core Code First
- Migrations in `CRM.Infrastructure/Persistence/Migrations/`
- Connection string key: `ConnectionStrings:Default` in appsettings.json
- JWT config keys: `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes`
