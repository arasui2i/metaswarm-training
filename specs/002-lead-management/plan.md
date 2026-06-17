# Lead Management — Execution Plan

## Overview

Implement Lead Management following the same Clean Architecture + CQRS pattern used for Customer Management. The Lead entity tracks potential customers before conversion.

---

## Phase 1: Domain Layer

### 1.1 Lead Status Enum
**File:** `src/CRM.Domain/Enums/LeadStatus.cs`

```
New, Contacted, Qualified, Unqualified, Converted
```

### 1.2 Lead Entity
**File:** `src/CRM.Domain/Entities/Lead.cs`

Extend `BaseEntity`. Fields:
- `FirstName` (string, required)
- `LastName` (string, nullable)
- `CompanyName` (string, required)
- `Email` (string, required)
- `Phone` (string, nullable)
- `Status` (LeadStatus enum, default: New)
- `OwnerId` (Guid, nullable — FK to User)

---

## Phase 2: Application Layer

### 2.1 DTOs
**File:** `src/CRM.Application/Features/Leads/LeadDtos.cs`

- `LeadSummaryDto` — Id, FullName, CompanyName, Email, Status (for list/grid)
- `LeadDetailDto` — all fields (for get by id / edit form)

### 2.2 Create Lead
**Folder:** `src/CRM.Application/Features/Leads/CreateLead/`

- `CreateLeadCommand.cs` — `IRequest<Guid>` record (FirstName, LastName, CompanyName, Email, Phone, Status, OwnerId)
- `CreateLeadValidator.cs` — FluentValidation: FirstName required, CompanyName required, Email required + valid format
- `CreateLeadHandler.cs` — maps command → Lead entity, calls `ILeadRepository.AddAsync`, returns Id

### 2.3 Update Lead
**Folder:** `src/CRM.Application/Features/Leads/UpdateLead/`

- `UpdateLeadCommand.cs` — `IRequest` record (Id + all updatable fields)
- `UpdateLeadValidator.cs` — same rules as create
- `UpdateLeadHandler.cs` — fetches lead via `GetByIdAsync`, updates fields, calls `UpdateAsync`

### 2.4 Get Lead By Id
**Folder:** `src/CRM.Application/Features/Leads/GetLeadById/`

- `GetLeadByIdQuery.cs` — `IRequest<LeadDetailDto>` record (Id)
- `GetLeadByIdHandler.cs` — calls `GetByIdAsync`, throws `NotFoundException` if null, maps to `LeadDetailDto`

### 2.5 Search Leads
**Folder:** `src/CRM.Application/Features/Leads/GetLeads/`

- `GetLeadsQuery.cs` — `IRequest<PagedResult<LeadSummaryDto>>` record (Search, Page = 1, PageSize = 10)
- `GetLeadsHandler.cs` — calls `GetPagedAsync`, maps entities to `LeadSummaryDto`

### 2.6 Delete Lead
**Folder:** `src/CRM.Application/Features/Leads/DeleteLead/`

- `DeleteLeadCommand.cs` — `IRequest` record (Id)
- `DeleteLeadHandler.cs` — fetches lead, calls `SoftDeleteAsync`

### 2.7 Repository Interface
**File:** `src/CRM.Application/Interfaces/ILeadRepository.cs`

Methods:
- `GetByIdAsync(Guid id)`
- `GetPagedAsync(string? search, int page, int pageSize)`
- `AddAsync(Lead lead)`
- `UpdateAsync(Lead lead)`
- `SoftDeleteAsync(Guid id)`

---

## Phase 3: Infrastructure Layer

### 3.1 EF Core Configuration
**File:** `src/CRM.Infrastructure/Persistence/Configurations/LeadConfiguration.cs`

- Table name: `Leads`
- Required fields: FirstName, CompanyName, Email
- Default value for Status: New
- OwnerId as nullable FK

### 3.2 DbContext Update
**File:** `src/CRM.Infrastructure/Persistence/AppDbContext.cs`

Add: `DbSet<Lead> Leads`

### 3.3 Lead Repository
**File:** `src/CRM.Infrastructure/Repositories/LeadRepository.cs`

Implement `ILeadRepository`:
- `GetPagedAsync` — filter by search on FirstName, LastName, CompanyName, Email; order by CreatedAt desc; exclude soft-deleted
- `SoftDeleteAsync` — set `IsDeleted = true` (add `IsDeleted` to Lead entity)

### 3.4 DI Registration
**File:** `src/CRM.API/Program.cs`

Add: `builder.Services.AddScoped<ILeadRepository, LeadRepository>();`

### 3.5 Database Migration
Run after all infrastructure changes:
```
dotnet ef migrations add AddLeadTable --project src/CRM.Infrastructure --startup-project src/CRM.API
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.API
```

---

## Phase 4: API Layer

### 4.1 Authorization Policies
**File:** `src/CRM.API/Program.cs`

Add policies: `leads.view`, `leads.create`, `leads.edit`, `leads.delete`

### 4.2 Leads Controller
**File:** `src/CRM.API/Controllers/LeadsController.cs`

| Method | Route | Handler | Policy |
|--------|-------|---------|--------|
| GET | `/api/leads` | `GetLeadsQuery` | leads.view |
| GET | `/api/leads/{id}` | `GetLeadByIdQuery` | leads.view |
| POST | `/api/leads` | `CreateLeadCommand` | leads.create |
| PUT | `/api/leads/{id}` | `UpdateLeadCommand` | leads.edit |
| DELETE | `/api/leads/{id}` | `DeleteLeadCommand` | leads.delete |

---

## Phase 5: Frontend

### 5.1 API Client
**File:** `web/src/api/leads.ts`

Types: `LeadSummary`, `LeadDetail`, `CreateLeadPayload`, `UpdateLeadPayload`, `LeadsPagedResult`

Functions:
- `getLeads(params: { search?, page?, pageSize? })`
- `getLeadById(id: string)`
- `createLead(payload: CreateLeadPayload)`
- `updateLead(id: string, payload: UpdateLeadPayload)`
- `deleteLead(id: string)`

### 5.2 Lead List Page
**File:** `web/src/pages/Leads/LeadListPage.tsx`

Features:
- Search input with debounce
- Paginated MUI DataGrid/Table with columns: Name, Company, Email, Status
- "Add Lead" button → navigates to `/leads/new`
- Edit icon → navigates to `/leads/:id/edit`
- Delete icon → opens `DeleteConfirmDialog`, calls `deleteLead`
- Use React Query (`useQuery`) for data fetching with invalidation on mutation

### 5.3 Lead Form Page
**File:** `web/src/pages/Leads/LeadFormPage.tsx`

Shared create/edit form:
- Fields: First Name, Last Name, Company, Email, Phone, Status (MUI Select)
- React Hook Form for form state
- On load (edit): fetch lead by id, populate form
- On submit: call `createLead` or `updateLead` via `useMutation`, redirect to list on success
- Client-side validation: FirstName, CompanyName, Email required; Email format

### 5.4 Routing
**File:** `web/src/App.tsx` (or router config)

Add routes:
```
/leads              → LeadListPage
/leads/new          → LeadFormPage (create mode)
/leads/:id/edit     → LeadFormPage (edit mode)
```

### 5.5 Navigation
**File:** `web/src/components/Layout/` (sidebar/nav component)

Add "Leads" nav item linking to `/leads`.

---

## Phase 6: Tests

### 6.1 Backend Unit Tests
**Project:** `src/CRM.Tests/`

- `CreateLeadValidatorTests.cs` — validate required fields and email format
- `CreateLeadHandlerTests.cs` — verify entity creation and repository call
- `GetLeadsHandlerTests.cs` — verify paging and search filtering

### 6.2 Frontend Tests
**Location:** alongside page components (`*.test.tsx`)

- `LeadListPage.test.tsx` — renders list, search triggers refetch, delete confirm dialog
- `LeadFormPage.test.tsx` — form validation errors shown, submit calls correct API

---

## Execution Order

1. Domain: LeadStatus enum → Lead entity
2. Application: ILeadRepository interface → DTOs → all Commands/Queries/Handlers
3. Infrastructure: LeadConfiguration → DbContext update → LeadRepository → DI registration → migration
4. API: authorization policies → LeadsController
5. Frontend: API client → LeadListPage → LeadFormPage → routing → navigation
6. Tests: validators, handlers, page components
