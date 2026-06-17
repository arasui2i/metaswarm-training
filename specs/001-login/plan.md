# Login — Technical Implementation Plan

## Stack
- **Backend:** ASP.NET Core 9, EF Core (Code First), SQL Server, Clean Architecture, CQRS + MediatR, FluentValidation, JWT
- **Frontend:** React + TypeScript, Material UI, React Hook Form, React Query

---

## Backend

### 1. Domain Layer (`CRM.Domain`)

**Entities**
- `User` — Id, Email, Username, PasswordHash, IsActive, CreatedAt
- `Role` — Id, Name
- `Permission` — Id, Name, ActionKey (e.g. `customers.view`, `customers.edit`)
- `UserRole` — UserId, RoleId (junction)
- `RolePermission` — RoleId, PermissionId (junction)

### 2. Infrastructure Layer (`CRM.Infrastructure`)

**EF Core**
- `AppDbContext` with DbSets for all entities above
- Code-first migration: `InitialLoginSchema`
- Seed data: default roles (`Admin`, `Sales`, `Viewer`) and permissions

**Services**
- `IJwtService` / `JwtService` — generates access token (claims: userId, email, roles)
- Token config in `appsettings.json`: `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes`

**Repository**
- `IUserRepository` / `UserRepository` — `GetByEmailAsync`, `GetByUsernameAsync`, `GetWithRolesAsync`

### 3. Application Layer (`CRM.Application`)

**LoginCommand** (`Features/Auth/Login/`)
```
LoginCommand
  - EmailOrUsername : string
  - Password        : string
  - RememberMe      : bool

LoginCommandResult
  - AccessToken  : string
  - ExpiresAt    : DateTime
  - User         : { Id, Email, Username, Roles[] }
```

- `LoginCommandValidator` (FluentValidation)
  - `EmailOrUsername`: NotEmpty
  - `Password`: NotEmpty, MinLength(6)

- `LoginCommandHandler`
  1. Resolve user by email or username via `IUserRepository`
  2. Verify password hash (BCrypt)
  3. Load roles + permissions
  4. Generate JWT via `IJwtService` — embed roles as claims
  5. If `RememberMe`, set longer expiry (7 days vs 1 day)
  6. Return `LoginCommandResult`

### 4. API Layer (`CRM.API`)

**Controller:** `AuthController` — `POST /api/auth/login`
- Accepts `LoginRequest` DTO
- Sends `LoginCommand` via MediatR
- Returns `200 OK` with token payload, or `401 Unauthorized`

**JWT Middleware** (in `Program.cs`)
- `AddAuthentication(JwtBearer)` with key/issuer/audience from config
- `AddAuthorization` with named policies mapped to permissions (e.g. `Policy("customers.view")`)

**Role-Action Mapping**
- `PermissionAuthorizationHandler` — custom `IAuthorizationHandler`
- `[Authorize(Policy = "customers.view")]` on downstream endpoints

---

## Frontend

### 1. Auth API (`src/api/auth.ts`)
- `loginApi(payload: LoginRequest): Promise<LoginResponse>` — POST `/api/auth/login`
- Token storage helper: `localStorage` if RememberMe, `sessionStorage` otherwise

### 2. React Query Hook (`src/hooks/useLogin.ts`)
- `useMutation` wrapping `loginApi`
- On success: store token, set user in auth context, navigate to `/customers`
- On error: surface `401` as "Invalid credentials"

### 3. Auth Context (`src/context/AuthContext.tsx`)
- Stores `{ user, roles, token, isAuthenticated }`
- `AuthProvider` reads token from storage on mount (rehydrate session)
- Exposes `logout()` — clears storage + context

### 4. Protected Route (`src/components/ProtectedRoute.tsx`)
- Wraps routes requiring auth; redirects to `/login` if not authenticated

### 5. Login Page (`src/pages/Login/LoginPage.tsx`)

**Layout (Material UI)**
- Split-panel: illustration left, form right (matches mockup)
- Heading: "Welcome Back :)"
- Subtext: prompt to login with email and password

**Form (React Hook Form)**
```
Fields:
  - Email Address  (type="email", required, email format)
  - Password       (type="password", required, minLength 6)
  - Remember Me    (Checkbox)

Actions:
  - "Login Now" button (primary, submits form)
  - "Forgot Password?" link (navigates to /forgot-password — stub route)
```

**Validation errors** shown inline below each field via MUI `helperText`.

### 6. Routing (`src/App.tsx`)
```
/login          → LoginPage (public)
/customers      → CustomerListPage (protected)
/               → redirect to /customers
```

---

## File Structure

```
CRM.Domain/
  Entities/User.cs
  Entities/Role.cs
  Entities/Permission.cs

CRM.Infrastructure/
  Persistence/AppDbContext.cs
  Persistence/Migrations/
  Repositories/UserRepository.cs
  Services/JwtService.cs

CRM.Application/
  Features/Auth/Login/LoginCommand.cs
  Features/Auth/Login/LoginCommandHandler.cs
  Features/Auth/Login/LoginCommandValidator.cs

CRM.API/
  Controllers/AuthController.cs
  Authorization/PermissionAuthorizationHandler.cs

src/
  api/auth.ts
  hooks/useLogin.ts
  context/AuthContext.tsx
  components/ProtectedRoute.tsx
  pages/Login/LoginPage.tsx
```

---

## Implementation Order

1. Domain entities + EF migration
2. `JwtService` + `UserRepository`
3. `LoginCommand` + handler + validator
4. `AuthController` + JWT middleware wiring
5. `AuthContext` + token storage helper
6. `LoginPage` UI + form
7. `useLogin` mutation + post-login redirect
8. `ProtectedRoute` + routing

---

## Testing

**Backend (nUnit)**
- `LoginCommandHandlerTests` — valid login, wrong password, unknown user, RememberMe expiry difference
- `LoginCommandValidatorTests` — empty fields, invalid email format

**Frontend (Vitest + RTL)**
- `LoginPage` — renders fields, shows validation errors, calls API on submit
- `useLogin` — success stores token and redirects, 401 shows error message
- `ProtectedRoute` — redirects unauthenticated users
