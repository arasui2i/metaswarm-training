# Project Context (Maintained by Orchestrator)

## Tooling
- Backend: .NET 9 / ASP.NET Core 9
- ORM: EF Core 9 Code First (SQL Server)
- Test runner: nUnit + Moq (CRM.Tests) + coverlet for coverage
- Coverage gate: 80% line + branch (backend: coverlet.runsettings excludes EF persistence/migrations)
- Frontend package manager: npm
- Frontend test runner: Vitest 4.x + React Testing Library 16 (run from src/)
- Frontend build: Vite 8 / vitest/config
- Frontend linter: TypeScript strict (tsconfig.app.json), eslint
- Frontend coverage gate: 80% line + branch (v8 provider; excludes App.tsx, api/**, stub pages)

## Solution Layout
```
CRM.sln
CRM.Domain/           — Entities only, no deps
CRM.Application/      — CQRS, MediatR, FluentValidation, interfaces
CRM.Infrastructure/   — EF Core, repositories, JWT service
CRM.API/              — Controllers, Program.cs, middleware
CRM.Tests/            — nUnit tests
src/                  — React + TypeScript frontend (npm project root)
  src/                — Vite source root (main.tsx, App.tsx, etc.)
    api/              — Axios client + API functions
    context/          — AuthContext
    hooks/            — useLogin, etc.
    components/       — ProtectedRoute, shared UI
    pages/            — Route-level pages
      Login/          — LoginPage.tsx
      Customers/      — stub
      ForgotPassword/ — stub
    __tests__/        — Vitest test files
specs/                — Feature specs (source of truth)
coverlet.runsettings  — Coverage exclusion config (EF code excluded)
```

## Key Patterns
- CQRS: each feature = Command/Query record + Handler in CRM.Application/Features/{Domain}/{Action}/
- Interfaces: IJwtService, IUserRepository defined in CRM.Application/Interfaces/
- DI registration: all services in Program.cs
- JWT secret: dotnet user-secrets for local dev; `appsettings.json` has placeholder "REPLACE_WITH_USER_SECRET"
- Axios: src/src/api/client.ts — base instance + Bearer interceptor; token in localStorage (rememberMe) or sessionStorage
- React import path rule: imports from src/src/pages/**/ to src/src/hooks/ need `../../hooks/` (two levels up)
- MUI v9: use `slotProps={{ input: { endAdornment: ... } }}` NOT deprecated `InputProps`

## Completed Work Units
| WU | Title | Key Files | Services/Patterns |
|----|-------|-----------|-------------------|
| WU-01 | Domain Entities | CRM.Domain/Entities/User, Role, Permission, UserRole, RolePermission | POCO entities, composite PKs for join tables |
| WU-02 | EF Core + Migrations | CRM.Infrastructure/Persistence/AppDbContext, AppDbContextFactory, Migrations/ | Seed: Admin/Sales/Viewer roles + customers.view/edit permissions |
| WU-03 | JWT Service | CRM.Infrastructure/Services/JwtService, CRM.Application/Interfaces/IJwtService | HMAC-SHA256, rememberMe=true→7d/false→1d, Sub+Email+Role claims |
| WU-04 | User Repository | CRM.Infrastructure/Repositories/UserRepository, IUserRepository | GetByEmail (IsActive filter), GetByUsername, GetWithRoles (Include chain) |
| WU-05 | Login Command | CRM.Application/Features/Auth/Login/{LoginCommand, LoginCommandHandler, LoginCommandValidator} | BCrypt.Verify, email→username fallback, UnauthorizedException |
| WU-06 | Auth Controller | CRM.API/Controllers/AuthController | [EnableRateLimiting("auth-login")], fixed-window 10/min IP-keyed |
| WU-07 | Permission RBAC | CRM.API/Authorization/{PermissionRequirement, PermissionAuthorizationHandler} | Checks ClaimTypes.Role against ActionKey, registered as Singleton IAuthorizationHandler |
| WU-08 | Backend Tests | CRM.Tests/Auth/{LoginCommandHandlerTests, LoginCommandValidatorTests, JwtServiceTests, DomainEntityTests} | 28 tests, 100% line, 85.7% branch; coverlet.runsettings excludes EF persistence |
| WU-09 | Axios + AuthContext | src/src/api/client.ts, auth.ts, context/AuthContext.tsx, main.tsx | setToken/getToken/clearToken, JWT rehydration on mount, AuthProvider wraps App |
| WU-10 | Auth Context Tests | (covered within WU-15 AuthContext.test.tsx) | — |
| WU-11 | useLogin Hook | src/src/hooks/useLogin.ts | useMutation, onSuccess: setToken+setAuth+navigate('/customers') |
| WU-12 | ProtectedRoute | src/src/components/ProtectedRoute.tsx | isAuthenticated → Outlet, else Navigate to /login replace |
| WU-13 | Login Page UI | src/src/pages/Login/LoginPage.tsx | MUI v9 split-panel, RHF, slotProps.input eye-toggle, rememberMe checkbox, isPending/isError states |
| WU-14 | Routing Setup | src/src/App.tsx, pages/Customers/CustomersPage.tsx, pages/ForgotPassword/ForgotPasswordPage.tsx | BrowserRouter, /login, /customers (ProtectedRoute), /forgot-password, /→/customers, *→/login |
| WU-15 | Frontend Tests | src/src/__tests__/{LoginPage, useLogin, ProtectedRoute, AuthContext}.test.tsx, src/vite.config.ts | 21 tests, 97.4% line, 83.3% branch; vi.mock factories must not reference outer const vars (TDZ issue) |

## Established Patterns
- Backend test mocking: Moq `Mock<IInterface>`, setup with `.ReturnsAsync()`, verify with `.Verify()`
- BCrypt in tests: call `BCrypt.HashPassword(raw)` in SetUp, never hardcode hashes
- JwtService test: `Mock<IConfiguration>` with `.Setup(x => x["Jwt:Key"]).Returns(...)` indexer syntax
- Frontend test mocking: `vi.mock(path)` at top, import mocked module after, use `vi.mocked(fn).mockReturnValue(...)` in `beforeEach`
- vi.mock hoisting: factory function must NOT directly reference outer `const` variables (TDZ); use `vi.fn()` directly in factory, then `vi.mocked()` to control per-test
- MUI label queries in RTL: use exact string `getByLabelText('Password')` not `/password/i` regex when an icon button also has `aria-label="Show password"`
- Coverage config path: vitest exclude paths are relative to vite.config.ts location (e.g., `'src/App.tsx'` = `src/src/App.tsx` resolved)

## Next Module
- specs/002-lead-management — not started
- Run `/metaswarm:start` with 002-lead-management to begin
