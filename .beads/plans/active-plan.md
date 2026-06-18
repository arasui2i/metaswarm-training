# Active Plan — 001-login: Login Module
<!-- approved: 2026-06-18 -->
<!-- gate-iterations: 2 -->
<!-- user-approved: true -->
<!-- status: in-progress -->

## Epic
Beads ID: metaswarm-training-9yq
Spec: specs/001-login/spec.md | Plan: specs/001-login/plan.md | Tasks: specs/001-login/task.md

---

## Work Unit Decomposition

### Backend (sequential chain with parallel branches)

| WU | Beads ID | Title | Depends On | File Scope | Checkpoint |
|---|---|---|---|---|---|
| WU-01 | metaswarm-training-uvk | Domain Entities | — | CRM.Domain/Entities/ | No |
| WU-02 | metaswarm-training-m9c | EF Core DbContext + Migration + Seed | WU-01 | CRM.Infrastructure/Persistence/ | No |
| WU-03 | metaswarm-training-4tk | JWT Service | WU-01 | CRM.Infrastructure/Services/, CRM.Application/Interfaces/ | No |
| WU-04 | metaswarm-training-9ko | User Repository | WU-02 | CRM.Infrastructure/Repositories/, CRM.Application/Interfaces/ | No |
| WU-05 | metaswarm-training-p1x | LoginCommand CQRS | WU-03, WU-04 | CRM.Application/Features/Auth/Login/ | Yes — checkpoint after |
| WU-06 | metaswarm-training-6gm | AuthController + JWT Middleware | WU-05 | CRM.API/Controllers/, CRM.API/Program.cs | No |
| WU-07 | metaswarm-training-1xh | Permission Authorization Handler | WU-06 | CRM.API/Authorization/, CRM.API/Program.cs | No |
| WU-08 | metaswarm-training-eb1 | Backend Unit Tests | WU-05 | CRM.Tests/ | Yes — checkpoint after |

### Frontend (sequential chain with parallel branches at WU-11/WU-12)

| WU | Beads ID | Title | Depends On | File Scope | Checkpoint |
|---|---|---|---|---|---|
| WU-09 | metaswarm-training-fct | Auth API + Token Storage | — (parallel with backend) | src/api/auth.ts, src/api/client.ts | No |
| WU-10 | metaswarm-training-end | Auth Context | WU-09 | src/context/AuthContext.tsx, src/main.tsx | No |
| WU-11 | metaswarm-training-86f | useLogin Hook | WU-10 | src/hooks/useLogin.ts | No |
| WU-12 | metaswarm-training-44s | Protected Route | WU-10 | src/components/ProtectedRoute.tsx | No |
| WU-13 | metaswarm-training-gvo | Login Page UI + Form Wiring | WU-11, WU-12 | src/pages/Login/ | No |
| WU-14 | metaswarm-training-bmf | Routing Setup | WU-12, WU-13 | src/App.tsx | No |
| WU-15 | metaswarm-training-8e0 | Frontend Tests | WU-11, WU-13, WU-14 | src/__tests__/ or co-located *.test.tsx | Yes — final checkpoint |

---

## Execution Order (DAG)

```
WU-01 ──→ WU-02 ──→ WU-04 ──┐
       └──→ WU-03 ────────────┴──→ WU-05 ──→ WU-06 ──→ WU-07
                                        └──────────────→ WU-08

WU-09 ──→ WU-10 ──→ WU-11 ──┐
                  └──→ WU-12 ─┴──→ WU-13 ──→ WU-14 ──→ WU-15
```

WU-02 and WU-03 run in **parallel** after WU-01.
WU-11 and WU-12 run in **parallel** after WU-10.
Frontend chain (WU-09...) runs in **parallel** with backend chain (WU-01...).

---

## API Contract

### POST /api/auth/login

- **Request Body**: `{ emailOrUsername: string, password: string, rememberMe: bool }`
  - `emailOrUsername`: required, non-empty
  - `password`: required, non-empty, minLength 6
- **Success**: `200 OK` → `{ accessToken: string, expiresAt: DateTime, user: { id, email, username, roles: string[] } }`
- **Errors**:
  - `400 Bad Request` — FluentValidation failure (missing/invalid fields)
  - `401 Unauthorized` — wrong password or unknown user
  - `429 Too Many Requests` — rate limit exceeded (10 req/min, fixed-window, keyed by client IP via ASP.NET Core built-in `AddRateLimiter`)

---

## Security Considerations

| Concern | Mitigation |
|---|---|
| Input validation | FluentValidation on LoginCommand — NotEmpty, MinLength(6) |
| Brute force | Rate limiting: 10 requests/minute on /api/auth/login |
| Password storage | BCrypt hash verification (never plaintext) |
| JWT secret | Jwt:Key via `dotnet user-secrets` (local dev) and env var `Jwt__Key` (CI/prod); appsettings.json holds placeholder only — never a real key |
| Token expiry | 1 day default; 7 days with RememberMe |
| RBAC | PermissionAuthorizationHandler + named policies per action key |

---

## User Flows

### Login Flow
```
User → /login page
  → fills Email + Password (+ RememberMe checkbox)
  → clicks "Login Now"
  → POST /api/auth/login
  → [success] store token in localstorage → update AuthContext → navigate to /customers
  → [401] show "Invalid email or password" below form
  → [validation error] show inline field errors
```

### Protected Route
```
User → navigates to /customers (unauthenticated)
  → ProtectedRoute checks isAuthenticated
  → redirect to /login
```

### Session Rehydration
```
User → refreshes browser
  → AuthContext.mount: check localStorage (rememberMe=true) or sessionStorage
  → if token found → restore user state → stay on protected route
  → if no token → ProtectedRoute redirects to /login
```

---

## Human Checkpoints

1. **After WU-05** (LoginCommand complete): Verify CQRS handler logic before wiring into API layer
2. **After WU-08** (Backend tests complete): Verify test coverage ≥ 20% before frontend work
3. **After WU-15** (All done): Final review before marking module complete

---

## Definition of Done — Per Work Unit

### WU-01 — Domain Entities
- [ ] User.cs: Id(Guid), Email, Username, PasswordHash, IsActive(bool), CreatedAt(DateTime)
- [ ] Role.cs: Id(Guid), Name
- [ ] Permission.cs: Id(Guid), Name, ActionKey
- [ ] UserRole.cs: UserId, RoleId (junction, no nav props needed)
- [ ] RolePermission.cs: RoleId, PermissionId (junction)
- [ ] No infrastructure/framework dependencies in CRM.Domain

### WU-02 — EF Core DbContext + Migration + Seed
- [ ] AppDbContext with DbSet<User>, DbSet<Role>, DbSet<Permission>, DbSet<UserRole>, DbSet<RolePermission>
- [ ] Migration named InitialLoginSchema generated and applies cleanly
- [ ] Seed: Admin, Sales, Viewer roles
- [ ] Seed: customers.view, customers.edit permissions
- [ ] Seed: Admin role gets both permissions

### WU-03 — JWT Service
- [ ] IJwtService interface defined in CRM.Application/Interfaces/
- [ ] JwtService.GenerateToken embeds userId, email, roles[] as claims
- [ ] RememberMe=true → 7-day expiry; false → 1-day expiry (uses ExpiryMinutes from config)
- [ ] appsettings.json has Jwt:Key, Jwt:Issuer, Jwt:Audience, Jwt:ExpiryMinutes placeholders
- [ ] Registered in DI in Program.cs

### WU-04 — User Repository
- [ ] IUserRepository in CRM.Application/Interfaces/ with GetByEmailAsync, GetByUsernameAsync, GetWithRolesAsync
- [ ] UserRepository implements IUserRepository using EF Core AppDbContext
- [ ] Registered in DI in Program.cs

### WU-05 — LoginCommand CQRS
- [ ] LoginCommand record with EmailOrUsername, Password, RememberMe
- [ ] LoginCommandResult record with AccessToken, ExpiresAt, User dto
- [ ] LoginCommandValidator: NotEmpty on both, email format on EmailOrUsername, MinLength(6) on Password
- [ ] LoginCommandHandler: resolves by email then username, BCrypt verify, loads roles, calls JwtService
- [ ] Handler throws UnauthorizedException on invalid credentials (not return null)

### WU-06 — AuthController + JWT Middleware
- [ ] AuthController.Login: POST /api/auth/login, sends LoginCommand via IMediator
- [ ] Returns 200+token on success, 401 on UnauthorizedException
- [ ] Program.cs: `AddRateLimiter` configured with fixed-window policy "auth-login": 10 permits/60s window, keyed by client IP
- [ ] `[EnableRateLimiting("auth-login")]` applied to the Login action; returns 429 on breach
- [ ] Program.cs: AddAuthentication(JwtBearer) with key/issuer/audience from IConfiguration
- [ ] Program.cs: AddAuthorization with at least customers.view and customers.edit policies
- [ ] appsettings.json Jwt:Key is a placeholder (`"REPLACE_WITH_USER_SECRET"`); real key set via `dotnet user-secrets set "Jwt:Key" "<dev-key>"`

### WU-07 — Permission Authorization Handler
- [ ] PermissionRequirement record/class wrapping ActionKey string
- [ ] PermissionAuthorizationHandler: checks user's role claims against permission's ActionKey
- [ ] Registered in DI; named policies wired in Program.cs

### WU-08 — Backend Unit Tests
- [ ] LoginCommandHandlerTests: valid login returns token, wrong password → 401, unknown user → 401, RememberMe=true produces longer expiry
- [ ] LoginCommandValidatorTests: empty EmailOrUsername fails, empty password fails, password < 6 chars fails, valid values pass
- [ ] JwtServiceTests: GenerateToken returns non-empty token string, token contains userId/email/roles claims, RememberMe=true sets expiry ≥ 7 days
- [ ] dotnet test passes with 0 failures
- [ ] Coverage ≥ 20% on CRM.Application project

### WU-09 — Auth API + Token Storage
- [ ] src/api/auth.ts exports loginApi(payload: LoginRequest): Promise<LoginResponse> using the Axios client
- [ ] Token storage helper: setToken(token, rememberMe), getToken(), clearToken() — localStorage when rememberMe, sessionStorage otherwise
- [ ] src/api/client.ts: Axios instance with baseURL from `import.meta.env.VITE_API_URL`
- [ ] Axios request interceptor on client.ts: reads token via getToken() and attaches `Authorization: Bearer <token>` header on every request

### WU-10 — Auth Context
- [ ] AuthContext provides { user, roles, token, isAuthenticated, logout }
- [ ] AuthProvider rehydrates from storage on mount
- [ ] logout() clears both storages and resets state
- [ ] AuthProvider wraps app in main.tsx

### WU-11 — useLogin Hook
- [ ] useLogin() returns useMutation result
- [ ] On success: setToken(), setUser in context, navigate('/customers')
- [ ] On 401: mutation error contains "Invalid email or password"

### WU-12 — Protected Route
- [ ] ProtectedRoute redirects to /login if !isAuthenticated
- [ ] Renders <Outlet /> or children if authenticated

### WU-13 — Login Page UI + Form Wiring
- [ ] Split-panel MUI Grid layout (left placeholder, right form)
- [ ] "Welcome Back :)" heading
- [ ] Email field (type=email, required, email format validation)
- [ ] Password field: type toggles between "password" and "text" via MUI InputAdornment eye-icon button (IconButton + Visibility/VisibilityOff icons)
- [ ] Password field validation: required, minLength 6
- [ ] Remember Me MUI Checkbox + FormControlLabel
- [ ] "Forgot Password?" MUI Link → /forgot-password
- [ ] "Login Now" primary MUI Button with loading state (disabled + CircularProgress when mutation is pending)
- [ ] React Hook Form connected; inline MUI helperText errors below each field
- [ ] useLogin mutation called on submit; API error (e.g. "Invalid email or password") shown below form in MUI Alert
- [ ] Social login buttons (Google, Facebook, Twitter) explicitly OUT OF SCOPE for v1

### WU-14 — Routing Setup
- [ ] React Router configured in src/App.tsx
- [ ] /login → LoginPage (public)
- [ ] /customers → stub CustomerListPage (protected via ProtectedRoute)
- [ ] /forgot-password → stub ForgotPasswordPage (renders "Coming soon", no API calls, no user input)
- [ ] / → &lt;Navigate to="/customers" /&gt;
- [ ] All protected routes wrapped with ProtectedRoute

### WU-15 — Frontend Tests
- [ ] LoginPage.test.tsx: renders email+password+rememberMe, shows field errors on empty submit, calls loginApi on valid submit
- [ ] useLogin.test.ts: success stores token and navigates, 401 returns "Invalid email or password"
- [ ] ProtectedRoute.test.tsx: unauthenticated → redirects to /login, authenticated → renders children
- [ ] vitest run passes with 0 failures
- [ ] Coverage ≥ 20% lines + branches on src/
