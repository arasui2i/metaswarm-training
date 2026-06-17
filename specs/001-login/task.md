# Login — Task Breakdown

## Backend

### [ ] T01 — Domain Entities
- Create `User` entity: Id, Email, Username, PasswordHash, IsActive, CreatedAt
- Create `Role` entity: Id, Name
- Create `Permission` entity: Id, Name, ActionKey
- Create `UserRole` junction entity: UserId, RoleId
- Create `RolePermission` junction entity: RoleId, PermissionId

### [ ] T02 — EF Core DbContext & Migration
- Add DbSets for all entities to `AppDbContext`
- Create initial Code First migration: `InitialLoginSchema`
- Seed default roles: `Admin`, `Sales`, `Viewer`
- Seed default permissions (e.g. `customers.view`, `customers.edit`)

### [ ] T03 — JWT Service
- Define `IJwtService` interface with `GenerateToken(user, roles, rememberMe)` method
- Implement `JwtService` — embed userId, email, roles as JWT claims
- Wire `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes` in `appsettings.json`
- Register `JwtService` in DI

### [ ] T04 — User Repository
- Define `IUserRepository` with `GetByEmailAsync`, `GetByUsernameAsync`, `GetWithRolesAsync`
- Implement `UserRepository` using EF Core
- Register in DI

### [ ] T05 — LoginCommand (CQRS)
- Create `LoginCommand` record: EmailOrUsername, Password, RememberMe
- Create `LoginCommandResult` record: AccessToken, ExpiresAt, User (Id, Email, Username, Roles)
- Create `LoginCommandValidator` (FluentValidation): NotEmpty on both fields, MinLength(6) on password
- Create `LoginCommandHandler`: resolve user → verify BCrypt hash → load roles → call JwtService → return result

### [ ] T06 — AuthController & JWT Middleware
- Create `AuthController` with `POST /api/auth/login` endpoint
- Add Rate limiting (10 requests/minute)
- Map request DTO to `LoginCommand`, send via MediatR
- Return `200 OK` with token payload; `401 Unauthorized` on failure
- Configure `AddAuthentication(JwtBearer)` + `AddAuthorization` in `Program.cs`

### [ ] T07 — Permission Authorization Handler
- Create `PermissionRequirement` (wraps ActionKey string)
- Implement `PermissionAuthorizationHandler` — checks role claims against required permission
- Register named policies (e.g. `Policy("customers.view")`) in `Program.cs`

### [ ] T08 — Backend Unit Tests (nUnit)
- `LoginCommandHandlerTests`: valid login, wrong password, unknown user, RememberMe expiry difference
- `LoginCommandValidatorTests`: empty fields, invalid email format, short password
---

## Frontend

### [ ] T09 — Auth API & Token Storage
- Create `src/api/auth.ts` — `loginApi(payload)` calling `POST /api/auth/login`
- Add token storage helper: `localStorage` if RememberMe, `sessionStorage` otherwise
- Add Axios instance with base URL + request interceptor attaching Bearer token

### [ ] T10 — Auth Context
- Create `src/context/AuthContext.tsx` with state: `{ user, roles, token, isAuthenticated }`
- On mount, rehydrate from storage (check both localStorage and sessionStorage)
- Expose `logout()` — clears storage and resets context state
- Wrap app in `AuthProvider` in `main.tsx`

### [ ] T11 — useLogin Hook
- Create `src/hooks/useLogin.ts` using `useMutation` (React Query)
- On success: store token via storage helper, update auth context, navigate to `/customers`
- On `401` error: return "Invalid email or password" message to caller

### [ ] T12 — Protected Route
- Create `src/components/ProtectedRoute.tsx`
- Redirect to `/login` if `isAuthenticated` is false
- Pass through children if authenticated

### [ ] T13 — Login Page UI
- Create `src/pages/Login/LoginPage.tsx`
- Split-panel layout (MUI Grid): illustration placeholder left, form right
- Heading "Welcome Back :)" + subtext (MUI Typography)
- Email Address field (MUI TextField, type="email")
- Password field (MUI TextField, type="password", toggle visibility)
- Remember Me checkbox (MUI Checkbox + FormControlLabel)
- "Forgot Password?" link (MUI Link, navigates to `/forgot-password`)
- "Login Now" primary button (submits form, shows loading state)

### [ ] T14 — Login Form Wiring
- Wire form with React Hook Form (`useForm`)
- Connect fields to validation rules: required, email format, minLength 6
- Show inline validation errors via MUI `helperText`
- Call `useLogin` mutation on submit
- Display API error (e.g. invalid credentials) below the form

### [ ] T15 — Routing Setup
- Configure React Router in `src/App.tsx`
- `/login` → `LoginPage` (public)
- `/customers` → `CustomerListPage` (protected, stub component for now)
- `/forgot-password` → stub page
- `/` → redirect to `/customers`
- Wrap protected routes with `ProtectedRoute`

### [ ] T16 — Frontend Tests (Vitest + RTL)
- `LoginPage.test.tsx`: renders all fields, shows validation errors on empty submit, calls API on valid submit
- `useLogin.test.ts`: success stores token + navigates, 401 returns error message
- `ProtectedRoute.test.tsx`: redirects unauthenticated, renders children when authenticated