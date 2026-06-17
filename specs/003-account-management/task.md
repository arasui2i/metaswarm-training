# Account Management — Task List

## Phase 1: Domain Layer

- [ ] **T-01** Create `AccountStatus` enum with values: `Active`, `Inactive`
  - File: `src/CRM.Domain/Enums/AccountStatus.cs`

- [ ] **T-02** Create `Account` entity extending `BaseEntity` with fields: `AccountName`, `Industry`, `Website`, `Phone`, `Status`, `IsDeleted` and navigation property `Contacts` (ICollection<Contact>)
  - File: `src/CRM.Domain/Entities/Account.cs`

- [ ] **T-03** Update `Contact` entity — change `AccountId` navigation property reference from `Customer` to `Account`
  - File: `src/CRM.Domain/Entities/Contact.cs`

---

## Phase 2: Application Layer

- [ ] **T-04** Create `IAccountRepository` interface with methods: `GetByIdAsync`, `GetPagedAsync`, `AddAsync`, `UpdateAsync`, `SoftDeleteAsync`
  - File: `src/CRM.Application/Interfaces/IAccountRepository.cs`

- [ ] **T-05** Create `AccountSummaryDto` (Id, AccountName, Industry, Phone) and `AccountDetailDto` (all fields including Status)
  - File: `src/CRM.Application/Features/Accounts/AccountDtos.cs`

- [ ] **T-06** Create `CreateAccountCommand`, `CreateAccountValidator`, `CreateAccountHandler`
  - Folder: `src/CRM.Application/Features/Accounts/CreateAccount/`
  - Validator rule: AccountName required

- [ ] **T-07** Create `UpdateAccountCommand`, `UpdateAccountValidator`, `UpdateAccountHandler`
  - Folder: `src/CRM.Application/Features/Accounts/UpdateAccount/`
  - Validator rule: same as create

- [ ] **T-08** Create `GetAccountByIdQuery` and `GetAccountByIdHandler`
  - Folder: `src/CRM.Application/Features/Accounts/GetAccountById/`
  - Throw `NotFoundException` if account not found

- [ ] **T-09** Create `GetAccountsQuery` and `GetAccountsHandler` with pagination and search
  - Folder: `src/CRM.Application/Features/Accounts/GetAccounts/`
  - Query params: `Search`, `Page` (default 1), `PageSize` (default 10)

- [ ] **T-10** Create `DeleteAccountCommand` and `DeleteAccountHandler`
  - Folder: `src/CRM.Application/Features/Accounts/DeleteAccount/`

---

## Phase 3: Infrastructure Layer

- [ ] **T-11** Create `AccountConfiguration` EF Core entity type configuration
  - File: `src/CRM.Infrastructure/Persistence/Configurations/AccountConfiguration.cs`
  - Table: `Accounts`, required field: AccountName, Status default: Active, one-to-many with `Contacts` (nullable FK `AccountId`)

- [ ] **T-12** Update `ContactConfiguration` — change FK reference from `Customers` to `Accounts` table
  - File: `src/CRM.Infrastructure/Persistence/Configurations/ContactConfiguration.cs`

- [ ] **T-13** Add `DbSet<Account> Accounts` to `AppDbContext`
  - File: `src/CRM.Infrastructure/Persistence/AppDbContext.cs`

- [ ] **T-14** Implement `AccountRepository` with all `IAccountRepository` methods
  - File: `src/CRM.Infrastructure/Repositories/AccountRepository.cs`
  - `GetPagedAsync`: filter on AccountName, Industry; exclude soft-deleted; order by AccountName asc

- [ ] **T-15** Register `IAccountRepository` → `AccountRepository` in DI container
  - File: `src/CRM.API/Program.cs`

- [ ] **T-16** Add and apply EF Core migration for Account table
  ```
  dotnet ef migrations add AddAccountTable --project src/CRM.Infrastructure --startup-project src/CRM.API
  dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.API
  ```

---

## Phase 4: API Layer

- [ ] **T-17** Add authorization policies: `accounts.view`, `accounts.create`, `accounts.edit`, `accounts.delete`
  - File: `src/CRM.API/Program.cs`

- [ ] **T-18** Create `AccountsController` with 5 endpoints
  - File: `src/CRM.API/Controllers/AccountsController.cs`
  - `GET /api/accounts` — `GetAccountsQuery` (accounts.view)
  - `GET /api/accounts/{id}` — `GetAccountByIdQuery` (accounts.view)
  - `POST /api/accounts` — `CreateAccountCommand` (accounts.create)
  - `PUT /api/accounts/{id}` — `UpdateAccountCommand` (accounts.edit)
  - `DELETE /api/accounts/{id}` — `DeleteAccountCommand` (accounts.delete)

---

## Phase 5: Frontend

- [ ] **T-19** Create accounts API client with types and axios functions
  - File: `web/src/api/accounts.ts`
  - Types: `AccountSummary`, `AccountDetail`, `CreateAccountPayload`, `UpdateAccountPayload`, `AccountsPagedResult`
  - Functions: `getAccounts`, `getAccountById`, `createAccount`, `updateAccount`, `deleteAccount`

- [ ] **T-20** Create `AccountListPage` with search, paginated table, add/edit/delete actions
  - File: `web/src/pages/Accounts/AccountListPage.tsx`
  - Columns: Account Name, Industry, Phone
  - Search with debounce, React Query for data fetching with cache invalidation on mutation

- [ ] **T-21** Create `AccountFormPage` for create and edit modes
  - File: `web/src/pages/Accounts/AccountFormPage.tsx`
  - Fields: Account Name, Industry, Website, Phone
  - React Hook Form, client-side validation (AccountName required), populate form on edit

- [ ] **T-22** Add Account routes to app router
  - File: `web/src/App.tsx`
  - Routes: `/accounts`, `/accounts/new`, `/accounts/:id/edit`

- [ ] **T-23** Add "Accounts" nav item to sidebar/navigation
  - File: `web/src/components/Layout/` (sidebar component)

- [ ] **T-24** Update Contact form Account dropdown to use `getAccounts` instead of `getCustomers`
  - File: `web/src/pages/Contacts/ContactFormPage.tsx`
  - Import `getAccounts` from `web/src/api/accounts.ts`

---

## Phase 6: Tests

- [ ] **T-25** Write `CreateAccountValidatorTests` — AccountName required rule
  - File: `src/CRM.Tests/Features/Accounts/CreateAccountValidatorTests.cs`

- [ ] **T-26** Write `CreateAccountHandlerTests` — entity creation and repository call
  - File: `src/CRM.Tests/Features/Accounts/CreateAccountHandlerTests.cs`

- [ ] **T-27** Write `GetAccountsHandlerTests` — paging and search filtering
  - File: `src/CRM.Tests/Features/Accounts/GetAccountsHandlerTests.cs`

- [ ] **T-28** Write `AccountListPage.test.tsx` — renders list, search, delete confirm dialog
  - File: `web/src/pages/Accounts/AccountListPage.test.tsx`

- [ ] **T-29** Write `AccountFormPage.test.tsx` — validation error on empty AccountName, submit calls correct API
  - File: `web/src/pages/Accounts/AccountFormPage.test.tsx`

---

## Summary

| Phase | Tasks | Count |
|-------|-------|-------|
| 1 — Domain | T-01 to T-03 | 3 |
| 2 — Application | T-04 to T-10 | 7 |
| 3 — Infrastructure | T-11 to T-16 | 6 |
| 4 — API | T-17 to T-18 | 2 |
| 5 — Frontend | T-19 to T-24 | 6 |
| 6 — Tests | T-25 to T-29 | 5 |
| **Total** | | **29** |
