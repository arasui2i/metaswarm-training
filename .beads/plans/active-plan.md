# Active Plan — 002-Lead-Management
<!-- approved: 2026-06-18 -->
<!-- gate-iterations: 1 -->
<!-- user-approved: true -->
<!-- status: completed -->

## Epic
Beads ID: metaswarm-training-e2p
Spec: specs/002-lead-management/spec.md | Plan: specs/002-lead-management/plan.md | Tasks: specs/002-lead-management/task.md

---

## Path Corrections (spec plan uses wrong paths)
- Backend: `CRM.Domain/`, `CRM.Application/`, `CRM.Infrastructure/`, `CRM.API/` (NOT `src/CRM.*`)
- Frontend: `src/src/` (NOT `web/src/`)
- No `BaseEntity` — Lead follows User.cs plain POCO pattern
- No sidebar yet — create `src/src/components/Layout/AppLayout.tsx` with Leads + Accounts nav

---

## Work Unit Decomposition

| WU | Beads ID | Title | Depends On | Checkpoint |
|---|---|---|---|---|
| WU-01 | metaswarm-training-e2p.1 | Domain: Lead entity + LeadStatus enum | — | No |
| WU-02 | metaswarm-training-e2p.2 | Application: CQRS, DTOs, ILeadRepository | WU-01 | No |
| WU-03 | metaswarm-training-e2p.3 | Infrastructure: LeadRepository + DbContext + DI | WU-02 | No |
| WU-04 | metaswarm-training-e2p.4 | EF Migration: AddLeadTable | WU-03 | No |
| WU-05 | metaswarm-training-e2p.5 | API: LeadsController + Policies | WU-04 | YES |
| WU-06 | metaswarm-training-e2p.6 | Backend Tests | WU-05 | YES |
| WU-07 | metaswarm-training-e2p.7 | Frontend API Client (leads.ts) | — | No |
| WU-08 | metaswarm-training-e2p.8 | Frontend Pages + Layout + Routing | WU-07 | No |
| WU-09 | metaswarm-training-e2p.9 | Frontend Tests | WU-08 | YES (FINAL) |

## Execution DAG

```
WU-01 → WU-02 → WU-03 → WU-04 → WU-05 → WU-06
                                              ↑
WU-07 → WU-08 → WU-09 (parallel with backend chain)
```

---

## API Contract

### GET /api/leads
- **Query**: `search?`, `page?=1`, `pageSize?=10`
- **Success**: `200 OK` → `{ items: LeadSummaryDto[], totalCount, page, pageSize }`
- **Auth**: leads.view policy

### GET /api/leads/{id}
- **Success**: `200 OK` → `LeadDetailDto`
- **Errors**: `404` (NotFoundException)
- **Auth**: leads.view policy

### POST /api/leads
- **Body**: `{ firstName, lastName?, companyName, email, phone?, status?, ownerId? }`
- **Success**: `201 Created` → `Guid` (new lead id)
- **Errors**: `400` (validation), `401` (unauthorized)
- **Auth**: leads.create policy

### PUT /api/leads/{id}
- **Body**: same fields + id
- **Success**: `204 No Content`
- **Errors**: `400`, `404`
- **Auth**: leads.edit policy

### DELETE /api/leads/{id}
- **Success**: `204 No Content`
- **Errors**: `404`
- **Auth**: leads.delete policy

---

## Human Checkpoints
1. **After WU-05** (API complete): Verify controller logic before tests
2. **After WU-06** (Backend tests): Verify 80% coverage before frontend
3. **After WU-09** (Module complete): Final review

---

## Definition of Done — Per WU

### WU-01 — Domain
- [ ] `CRM.Domain/Enums/LeadStatus.cs`: New, Contacted, Qualified, Unqualified, Converted
- [ ] `CRM.Domain/Entities/Lead.cs`: Id(Guid), FirstName, LastName(nullable), CompanyName, Email, Phone(nullable), Status(LeadStatus default New), OwnerId(Guid?), CreatedAt(DateTime), IsDeleted(bool)

### WU-02 — Application
- [ ] `CRM.Application/Common/Exceptions/NotFoundException.cs`
- [ ] `CRM.Application/Common/PagedResult.cs` — generic `PagedResult<T>` with Items, TotalCount, Page, PageSize
- [ ] `CRM.Application/Interfaces/ILeadRepository.cs` — 5 methods
- [ ] `CRM.Application/Features/Leads/LeadDtos.cs` — LeadSummaryDto, LeadDetailDto
- [ ] CreateLead: Command(IRequest<Guid>), Validator, Handler
- [ ] UpdateLead: Command(IRequest), Validator, Handler
- [ ] GetLeadById: Query(IRequest<LeadDetailDto>), Handler
- [ ] GetLeads: Query(IRequest<PagedResult<LeadSummaryDto>>), Handler
- [ ] DeleteLead: Command(IRequest), Handler
- [ ] `dotnet build` passes

### WU-03 — Infrastructure
- [ ] AppDbContext.cs: `DbSet<Lead> Leads` added, Lead configured in OnModelCreating
- [ ] LeadRepository.cs implements all 5 ILeadRepository methods
- [ ] GetPagedAsync: filter FirstName/LastName/CompanyName/Email, exclude IsDeleted, order CreatedAt desc
- [ ] SoftDeleteAsync: set IsDeleted=true
- [ ] Program.cs: `AddScoped<ILeadRepository, LeadRepository>()` registered
- [ ] leads.view/create/edit/delete policies added to AddAuthorization

### WU-04 — Migration
- [ ] EF migration file created (`AddLeadTable`)
- [ ] dotnet build passes after migration

### WU-05 — API (CHECKPOINT)
- [ ] LeadsController with 5 endpoints matching API contract
- [ ] NotFoundException → 404 response
- [ ] dotnet build passes

### WU-06 — Backend Tests (CHECKPOINT)
- [ ] CreateLeadValidatorTests: required fields, email format
- [ ] CreateLeadHandlerTests: happy path, AddAsync called
- [ ] GetLeadsHandlerTests: paging + search filtering
- [ ] dotnet test 0 failures
- [ ] Coverage ≥ 80%

### WU-07 — Frontend API
- [ ] leads.ts: LeadSummary, LeadDetail, payload types
- [ ] getLeads, getLeadById, createLead, updateLead, deleteLead functions

### WU-08 — Frontend Pages
- [ ] LeadListPage: MUI Table, Name/Company/Email/Status columns, search+debounce, Add/Edit/Delete actions
- [ ] LeadFormPage: RHF form, FirstName/LastName/Company/Email/Phone/Status fields, create+edit modes
- [ ] AppLayout: left sidebar with Leads + Accounts nav links
- [ ] App.tsx: /leads, /leads/new, /leads/:id/edit routes (protected, wrapped in AppLayout)

### WU-09 — Frontend Tests (FINAL CHECKPOINT)
- [ ] LeadListPage.test.tsx: renders list, search input present, delete confirm dialog
- [ ] LeadFormPage.test.tsx: validation errors shown, submit calls API
- [ ] npx vitest run 0 failures
- [ ] Coverage ≥ 80%
