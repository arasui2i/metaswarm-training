# Account Management — Execution Plan

## Overview

Implement Account Management following the same Clean Architecture + CQRS pattern used for Customer, Lead, and Contact features. An Account represents a customer organisation. The `Contact` entity's `AccountId` FK references this `Account` entity.

---

## Phase 1: Domain Layer

### 1.1 Account Status Enum
**File:** `src/CRM.Domain/Enums/AccountStatus.cs`

```
Active, Inactive
```

### 1.2 Account Entity
**File:** `src/CRM.Domain/Entities/Account.cs`

Extend `BaseEntity`. Fields:
- `AccountName` (string, required)
- `Industry` (string, nullable)
- `Website` (string, nullable)
- `Phone` (string, nullable)
- `Status` (AccountStatus enum, default: Active)
- `IsDeleted` (bool, default: false)

Navigation property:
- `Contacts` (ICollection<Contact>, one-to-many)

### 1.3 Update Contact Entity FK Reference
**File:** `src/CRM.Domain/Entities/Contact.cs`

Update `AccountId` navigation property type from `Customer` to `Account` so the FK correctly references the new `Account` entity.

---

## Phase 2: Application Layer

### 2.1 DTOs
**File:** `src/CRM.Application/Features/Accounts/AccountDtos.cs`

- `AccountSummaryDto` — Id, AccountName, Industry, Phone (for list/grid)
- `AccountDetailDto` — all fields including Status (for get by id / edit form)

### 2.2 Create Account
**Folder:** `src/CRM.Application/Features/Accounts/CreateAccount/`

- `CreateAccountCommand.cs` — `IRequest<Guid>` record (AccountName, Industry, Website, Phone, Status)
- `CreateAccountValidator.cs` — FluentValidation: AccountName required
- `CreateAccountHandler.cs` — maps command → Account entity, calls `IAccountRepository.AddAsync`, returns Id

### 2.3 Update Account
**Folder:** `src/CRM.Application/Features/Accounts/UpdateAccount/`

- `UpdateAccountCommand.cs` — `IRequest` record (Id + all updatable fields)
- `UpdateAccountValidator.cs` — same rules as create
- `UpdateAccountHandler.cs` — fetches account via `GetByIdAsync`, updates fields, calls `UpdateAsync`

### 2.4 Get Account By Id
**Folder:** `src/CRM.Application/Features/Accounts/GetAccountById/`

- `GetAccountByIdQuery.cs` — `IRequest<AccountDetailDto>` record (Id)
- `GetAccountByIdHandler.cs` — calls `GetByIdAsync`, throws `NotFoundException` if null, maps to `AccountDetailDto`

### 2.5 Search Accounts
**Folder:** `src/CRM.Application/Features/Accounts/GetAccounts/`

- `GetAccountsQuery.cs` — `IRequest<PagedResult<AccountSummaryDto>>` record (Search, Page = 1, PageSize = 10)
- `GetAccountsHandler.cs` — calls `GetPagedAsync`, maps entities to `AccountSummaryDto`

### 2.6 Delete Account
**Folder:** `src/CRM.Application/Features/Accounts/DeleteAccount/`

- `DeleteAccountCommand.cs` — `IRequest` record (Id)
- `DeleteAccountHandler.cs` — fetches account, calls `SoftDeleteAsync`

### 2.7 Repository Interface
**File:** `src/CRM.Application/Interfaces/IAccountRepository.cs`

Methods:
- `GetByIdAsync(Guid id)`
- `GetPagedAsync(string? search, int page, int pageSize)`
- `AddAsync(Account account)`
- `UpdateAsync(Account account)`
- `SoftDeleteAsync(Guid id)`

---

## Phase 3: Infrastructure Layer

### 3.1 EF Core Configuration
**File:** `src/CRM.Infrastructure/Persistence/Configurations/AccountConfiguration.cs`

- Table name: `Accounts`
- Required field: AccountName
- Default value for Status: Active
- Configure one-to-many: `Account` → `Contacts` (with nullable FK `AccountId`)

### 3.2 Update Contact Configuration
**File:** `src/CRM.Infrastructure/Persistence/Configurations/ContactConfiguration.cs`

Update FK reference from `Customers` to `Accounts` table to reflect the corrected relationship.

### 3.3 DbContext Update
**File:** `src/CRM.Infrastructure/Persistence/AppDbContext.cs`

Add: `DbSet<Account> Accounts`

### 3.4 Account Repository
**File:** `src/CRM.Infrastructure/Repositories/AccountRepository.cs`

Implement `IAccountRepository`:
- `GetPagedAsync` — filter by search on AccountName, Industry; exclude soft-deleted; order by AccountName asc
- `SoftDeleteAsync` — set `IsDeleted = true`

### 3.5 DI Registration
**File:** `src/CRM.API/Program.cs`

Add: `builder.Services.AddScoped<IAccountRepository, AccountRepository>();`

### 3.6 Database Migration
Run after all infrastructure changes:
```
dotnet ef migrations add AddAccountTable --project src/CRM.Infrastructure --startup-project src/CRM.API
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.API
```

---

## Phase 4: API Layer

### 4.1 Authorization Policies
**File:** `src/CRM.API/Program.cs`

Add policies: `accounts.view`, `accounts.create`, `accounts.edit`, `accounts.delete`

### 4.2 Accounts Controller
**File:** `src/CRM.API/Controllers/AccountsController.cs`

| Method | Route | Handler | Policy |
|--------|-------|---------|--------|
| GET | `/api/accounts` | `GetAccountsQuery` | accounts.view |
| GET | `/api/accounts/{id}` | `GetAccountByIdQuery` | accounts.view |
| POST | `/api/accounts` | `CreateAccountCommand` | accounts.create |
| PUT | `/api/accounts/{id}` | `UpdateAccountCommand` | accounts.edit |
| DELETE | `/api/accounts/{id}` | `DeleteAccountCommand` | accounts.delete |

### 4.3 Update Contact Form Account Dropdown Source
The Contact form's Account dropdown (`GET /api/customers` used as a placeholder in spec 004) should be updated to call `GET /api/accounts` once this feature is implemented.

---

## Phase 5: Frontend

### 5.1 API Client
**File:** `web/src/api/accounts.ts`

Types: `AccountSummary`, `AccountDetail`, `CreateAccountPayload`, `UpdateAccountPayload`, `AccountsPagedResult`

Functions:
- `getAccounts(params: { search?, page?, pageSize? })`
- `getAccountById(id: string)`
- `createAccount(payload: CreateAccountPayload)`
- `updateAccount(id: string, payload: UpdateAccountPayload)`
- `deleteAccount(id: string)`

### 5.2 Account List Page
**File:** `web/src/pages/Accounts/AccountListPage.tsx`

Features:
- Search input with debounce
- Paginated MUI Table with columns: Account Name, Industry, Phone
- "Add Account" button → navigates to `/accounts/new`
- Edit icon → navigates to `/accounts/:id/edit`
- Delete icon → opens `DeleteConfirmDialog`, calls `deleteAccount`
- React Query (`useQuery`) for data fetching with cache invalidation on mutation

### 5.3 Account Form Page
**File:** `web/src/pages/Accounts/AccountFormPage.tsx`

Shared create/edit form:
- Fields: Account Name, Industry, Website, Phone
- React Hook Form for form state
- On load (edit): fetch account by id, populate form
- On submit: call `createAccount` or `updateAccount` via `useMutation`, redirect to list on success
- Client-side validation: AccountName required

### 5.4 Routing
**File:** `web/src/App.tsx` (or router config)

Add routes:
```
/accounts              → AccountListPage
/accounts/new          → AccountFormPage (create mode)
/accounts/:id/edit     → AccountFormPage (edit mode)
```

### 5.5 Navigation
**File:** `web/src/components/Layout/` (sidebar/nav component)

Add "Accounts" nav item linking to `/accounts`.

### 5.6 Update Contact Form Account Dropdown
**File:** `web/src/pages/Contacts/ContactFormPage.tsx`

Switch Account dropdown data source from `getCustomers` to `getAccounts` (from `web/src/api/accounts.ts`).

---

## Phase 6: Tests

### 6.1 Backend Unit Tests
**Project:** `src/CRM.Tests/`

- `CreateAccountValidatorTests.cs` — AccountName required rule
- `CreateAccountHandlerTests.cs` — entity creation and repository call
- `GetAccountsHandlerTests.cs` — paging and search filtering

### 6.2 Frontend Tests
**Location:** alongside page components (`*.test.tsx`)

- `AccountListPage.test.tsx` — renders list, search triggers refetch, delete confirm dialog
- `AccountFormPage.test.tsx` — validation error on empty AccountName, submit calls correct API

---

## Execution Order

1. Domain: AccountStatus enum → Account entity → update Contact FK reference
2. Application: IAccountRepository interface → DTOs → all Commands/Queries/Handlers
3. Infrastructure: AccountConfiguration → update ContactConfiguration → DbContext update → AccountRepository → DI registration → migration
4. API: authorization policies → AccountsController → note Contact form dropdown update
5. Frontend: API client → AccountListPage → AccountFormPage → routing → navigation → update Contact form dropdown
6. Tests: validators, handlers, page components
