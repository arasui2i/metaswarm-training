---
name: crm-features
description: Feature catalog for the CRM project — scope, status, key files, and acceptance criteria per feature
metadata:
  type: features
---

# CRM Feature Catalog

## 001 — Login / Auth

**Spec:** `specs/001-login/spec.md` | **Plan:** `specs/001-login/plan.md` | **Tasks:** `specs/001-login/task.md`

### Scope
- JWT login with email/username + password + RememberMe
- Role-Permission RBAC: `Admin`, `Sales`, `Viewer` roles seeded at migration
- Permission policies map to action keys (e.g. `customers.view`, `customers.edit`)
- Token stored in `localStorage` (RememberMe) or `sessionStorage` (session-only)

### Key Files (Backend)
```
CRM.Domain/Entities/User.cs, Role.cs, Permission.cs
CRM.Infrastructure/Persistence/AppDbContext.cs
CRM.Infrastructure/Repositories/UserRepository.cs
CRM.Infrastructure/Services/JwtService.cs
CRM.Application/Features/Auth/Login/LoginCommand.cs
CRM.Application/Features/Auth/Login/LoginCommandHandler.cs
CRM.Application/Features/Auth/Login/LoginCommandValidator.cs
CRM.API/Controllers/AuthController.cs
CRM.API/Authorization/PermissionAuthorizationHandler.cs
```

### Key Files (Frontend)
```
src/src/api/client.ts         (Axios base instance + Bearer interceptor + setToken/getToken/clearToken)
src/src/api/auth.ts           (loginApi — POST /api/auth/login)
src/src/hooks/useLogin.ts     (useMutation → setToken + setAuth + navigate('/customers'))
src/src/context/AuthContext.tsx (AuthProvider, useAuth hook, JWT rehydration on mount)
src/src/components/ProtectedRoute.tsx
src/src/pages/Login/LoginPage.tsx  (MUI split-panel, RHF, password toggle, rememberMe)
src/src/App.tsx               (BrowserRouter, all routes wired)
```

### Status
- **COMPLETE** — all 15 work units closed, epic closed
- Backend tests: 28/28 passing, 100% line coverage
- Frontend tests: 21/21 passing, 97.4% line / 83.3% branch coverage

### API
- `POST /api/auth/login` → `{ accessToken, expiresAt, user: { id, email, username, roles[] } }`

### Acceptance Criteria
- Valid credentials → token returned, user navigated to `/customers`
- Wrong password → 401, "Invalid credentials" shown
- RememberMe true → 7-day token; false → 1-day token
- Unauthenticated access to protected routes → redirect to `/login`

---

## 002 — Lead Management

**Spec:** `specs/002-lead-management/spec.md` | **Plan:** `specs/002-lead-management/plan.md` | **Tasks:** `specs/002-lead-management/task.md`

### Scope
- Full CRUD for Leads
- Search/filter on list page
- OwnerId links lead to the logged-in user

### Entity Fields
`Id, FirstName, LastName, CompanyName, Email, Phone, Status, OwnerId, CreatedDate`

### Key Files (Backend)
```
CRM.Domain/Entities/Lead.cs
CRM.Application/Features/Leads/Create/CreateLeadCommand.cs + Handler + Validator
CRM.Application/Features/Leads/Update/UpdateLeadCommand.cs + Handler + Validator
CRM.Application/Features/Leads/GetById/GetLeadByIdQuery.cs + Handler
CRM.Application/Features/Leads/Search/SearchLeadsQuery.cs + Handler
CRM.Application/Features/Leads/Delete/DeleteLeadCommand.cs + Handler
CRM.API/Controllers/LeadsController.cs
```

### Key Files (Frontend)
```
src/api/leads.ts
src/hooks/useLeads.ts
src/pages/Leads/LeadListPage.tsx   (grid: Name, Company, Email, Status + search + add/edit/delete)
src/pages/Leads/LeadFormPage.tsx   (fields: FirstName, LastName, Company, Email, Phone, Status)
```

### APIs
- `POST /api/leads` — create
- `PUT /api/leads/{id}` — update
- `GET /api/leads/{id}` — get by id
- `GET /api/leads?search=` — search
- `DELETE /api/leads/{id}` — delete

### Validation
- `FirstName` required
- `CompanyName` required
- `Email` required

### Acceptance Criteria
- User can create, edit, search, and delete leads
- List shows Name, Company, Email, Status columns

---

## 003 — Account Management

**Spec:** `specs/003-account-management/spec.md` | **Plan:** `specs/003-account-management/plan.md` | **Tasks:** `specs/003-account-management/task.md`

### Scope
- Full CRUD for Accounts (customer organizations)
- Search/filter on list page

### Entity Fields
`Id, AccountName, Industry, Website, Phone, Status`

### Key Files (Backend)
```
CRM.Domain/Entities/Account.cs
CRM.Application/Features/Accounts/Create/CreateAccountCommand.cs + Handler + Validator
CRM.Application/Features/Accounts/Update/UpdateAccountCommand.cs + Handler + Validator
CRM.Application/Features/Accounts/GetById/GetAccountByIdQuery.cs + Handler
CRM.Application/Features/Accounts/Search/SearchAccountsQuery.cs + Handler
CRM.Application/Features/Accounts/Delete/DeleteAccountCommand.cs + Handler
CRM.API/Controllers/AccountsController.cs
```

### Key Files (Frontend)
```
src/api/accounts.ts
src/hooks/useAccounts.ts
src/pages/Accounts/AccountListPage.tsx  (grid: Account Name, Industry, Phone + search + add/edit/delete)
src/pages/Accounts/AccountFormPage.tsx  (fields: AccountName, Industry, Website, Phone)
```

### APIs
- `POST /api/accounts` — create
- `PUT /api/accounts/{id}` — update
- `GET /api/accounts/{id}` — get by id
- `GET /api/accounts?search=` — search
- `DELETE /api/accounts/{id}` — delete

### Validation
- `AccountName` required

### Acceptance Criteria
- User can create, edit, search, and delete accounts
