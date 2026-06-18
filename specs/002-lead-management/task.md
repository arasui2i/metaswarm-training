# Lead Management — Task List

## Phase 1: Domain Layer

- [x] **T-01** Create `LeadStatus` enum with values: `New`, `Contacted`, `Qualified`, `Unqualified`, `Converted`
  - File: `CRM.Domain/Enums/LeadStatus.cs`

- [x] **T-02** Create `Lead` entity extending `BaseEntity` with fields: `FirstName`, `LastName`, `CompanyName`, `Email`, `Phone`, `Status`, `OwnerId`, `IsDeleted`
  - File: `CRM.Domain/Entities/Lead.cs`

---

## Phase 2: Application Layer

- [x] **T-03** Create `ILeadRepository` interface with methods: `GetByIdAsync`, `GetPagedAsync`, `AddAsync`, `UpdateAsync`, `SoftDeleteAsync`
  - File: `CRM.Application/Interfaces/ILeadRepository.cs`

- [x] **T-04** Create `LeadSummaryDto` and `LeadDetailDto` records
  - File: `CRM.Application/Features/Leads/LeadDtos.cs`

- [x] **T-05** Create `CreateLeadCommand`, `CreateLeadValidator`, `CreateLeadHandler`
  - Folder: `CRM.Application/Features/Leads/CreateLead/`
  - Validator rules: FirstName required, CompanyName required, Email required + valid format

- [x] **T-06** Create `UpdateLeadCommand`, `UpdateLeadValidator`, `UpdateLeadHandler`
  - Folder: `CRM.Application/Features/Leads/UpdateLead/`

- [x] **T-07** Create `GetLeadByIdQuery` and `GetLeadByIdHandler`
  - Folder: `CRM.Application/Features/Leads/GetLeadById/`
  - Throw `NotFoundException` if lead not found

- [x] **T-08** Create `GetLeadsQuery` and `GetLeadsHandler` with pagination and search
  - Folder: `CRM.Application/Features/Leads/GetLeads/`
  - Query params: `Search`, `Page` (default 1), `PageSize` (default 10)

- [x] **T-09** Create `DeleteLeadCommand` and `DeleteLeadHandler`
  - Folder: `CRM.Application/Features/Leads/DeleteLead/`

---

## Phase 3: Infrastructure Layer

- [x] **T-10** Create `LeadConfiguration` EF Core entity type configuration
  - File: `CRM.Infrastructure/Persistence/AppDbContext.cs` (inline configuration in OnModelCreating)
  - Table: `Leads`, required fields, Status default, nullable OwnerId FK

- [x] **T-11** Add `DbSet<Lead> Leads` to `AppDbContext`
  - File: `CRM.Infrastructure/Persistence/AppDbContext.cs`

- [x] **T-12** Implement `LeadRepository` with all `ILeadRepository` methods
  - File: `CRM.Infrastructure/Repositories/LeadRepository.cs`
  - `GetPagedAsync`: filter on FirstName, LastName, CompanyName, Email; exclude soft-deleted; order by CreatedAt desc

- [x] **T-13** Register `ILeadRepository` → `LeadRepository` in DI container
  - File: `CRM.API/Program.cs`

- [x] **T-14** Add EF Core migration for Lead table
  - Migration: `CRM.Infrastructure/Migrations/20260618091346_AddLeadTable.cs`
  - Note: `dotnet ef database update` requires live SQL Server (not run in dev)

---

## Phase 4: API Layer

- [x] **T-15** Add authorization policies: `leads.view`, `leads.create`, `leads.edit`, `leads.delete`
  - File: `CRM.API/Program.cs`

- [x] **T-16** Create `LeadsController` with 5 endpoints
  - File: `CRM.API/Controllers/LeadsController.cs`
  - `GET /api/leads` — `GetLeadsQuery` (leads.view)
  - `GET /api/leads/{id}` — `GetLeadByIdQuery` (leads.view)
  - `POST /api/leads` — `CreateLeadCommand` (leads.create)
  - `PUT /api/leads/{id}` — `UpdateLeadCommand` (leads.edit)
  - `DELETE /api/leads/{id}` — `DeleteLeadCommand` (leads.delete)

---

## Phase 5: Frontend

- [x] **T-17** Create leads API client with types and axios functions
  - File: `src/src/api/leads.ts`
  - Types: `LeadSummary`, `LeadDetail`, `CreateLeadPayload`, `UpdateLeadPayload`, `LeadsPagedResult`
  - Functions: `getLeads`, `getLeadById`, `createLead`, `updateLead`, `deleteLead`

- [x] **T-18** Create `LeadListPage` with search, paginated table, add/edit/delete actions
  - File: `src/src/pages/Leads/LeadListPage.tsx`
  - Columns: Name, Company, Email, Status
  - Search with debounce, React Query for data fetching

- [x] **T-19** Create `LeadFormPage` for create and edit modes
  - File: `src/src/pages/Leads/LeadFormPage.tsx`
  - Fields: First Name, Last Name, Company, Email, Phone, Status
  - React Hook Form, client-side validation, populate form on edit

- [x] **T-20** Add Lead routes to app router
  - File: `src/src/App.tsx`
  - Routes: `/leads`, `/leads/new`, `/leads/:id/edit`

- [x] **T-21** Add "Leads" and "Accounts" nav items to sidebar
  - File: `src/src/components/Layout/AppLayout.tsx` (permanent MUI Drawer)

---

## Phase 6: Tests

- [x] **T-22** Write `CreateLeadValidatorTests` — required fields and email format
  - File: `CRM.Tests/Features/Leads/CreateLeadValidatorTests.cs`

- [x] **T-23** Write `CreateLeadHandlerTests` — entity creation and repository call
  - File: `CRM.Tests/Features/Leads/CreateLeadHandlerTests.cs`

- [x] **T-24** Write `GetLeadsHandlerTests` — paging and search filtering
  - File: `CRM.Tests/Features/Leads/GetLeadsHandlerTests.cs`

- [x] **T-25** Write `LeadListPage.test.tsx` — renders list, search, delete confirm dialog
  - File: `src/src/__tests__/LeadListPage.test.tsx`

- [x] **T-26** Write `LeadFormPage.test.tsx` — validation errors, submit calls correct API
  - File: `src/src/__tests__/LeadFormPage.test.tsx`

---

## Summary

| Phase | Tasks | Count | Status |
|-------|-------|-------|--------|
| 1 — Domain | T-01 to T-02 | 2 | ✅ Complete |
| 2 — Application | T-03 to T-09 | 7 | ✅ Complete |
| 3 — Infrastructure | T-10 to T-14 | 5 | ✅ Complete |
| 4 — API | T-15 to T-16 | 2 | ✅ Complete |
| 5 — Frontend | T-17 to T-21 | 5 | ✅ Complete |
| 6 — Tests | T-22 to T-26 | 5 | ✅ Complete |
| **Total** | | **26** | **✅ All Complete** |
