# CRM — Claude Code Project Guide

## Project

Fullstack CRM training project. **Specs live in `specs/`; code does not exist yet — build from specs.**

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core 9 |
| Architecture | Clean Architecture + CQRS + MediatR |
| ORM | EF Core 9 (Code First) |
| Database | SQL Server |
| Auth | JWT + Role-Permission RBAC |
| Validation | FluentValidation |
| Backend tests | nUnit + Moq |
| Frontend | React 18 + TypeScript |
| UI Library | Material UI v6 |
| Forms | React Hook Form |
| Data fetching | React Query v5 |
| Frontend tests | Vitest + React Testing Library |

## Solution Layout

```
CRM.sln
CRM.Domain/           — Entities, interfaces (no deps)
CRM.Application/      — CQRS handlers, validators, DTOs
CRM.Infrastructure/   — EF Core, repositories, JWT service
CRM.API/              — Controllers, Program.cs, middleware
CRM.Tests/            — nUnit tests
src/                  — React frontend
  api/                — API client functions
  hooks/              — React Query hooks
  context/            — Auth context
  components/         — Shared components (ProtectedRoute, etc.)
  pages/              — Route-level pages
specs/                — Feature specs (source of truth for requirements)
  001-login/
  002-lead-management/
  003-account-management/
```

## Key Conventions

- **New feature flow:** read `specs/{feature}/spec.md` → read `plan.md` → implement in order listed in plan
- **Backend feature folder:** `CRM.Application/Features/{Domain}/{Action}/` — Command + Handler + Validator
- **Frontend feature folder:** `src/pages/{Feature}/` + `src/hooks/use{Feature}.ts` + `src/api/{feature}.ts`
- **No comments** unless the WHY is non-obvious
- **No scaffolding beyond what the spec requires**

## Running the App

```bash
# Backend
cd CRM.API && dotnet run

# Frontend
cd src && npm run dev

# Database migration
cd CRM.API && dotnet ef database update
```

## Running Tests

```bash
# Backend
dotnet test

# Frontend
cd src && npx vitest run
cd src && npx vitest run --coverage   # with coverage
```

## Coverage Gate

80% line + branch coverage required on CI (GitHub Actions).

## Metaswarm

- Profile: `.metaswarm/project-profile.json`
- Knowledge base: `.metaswarm/knowledge-base/`
- CI: `.github/workflows/ci.yml`
- Adversarial review: Gemini CLI
- PR/issue automation: disabled (GitHub remote connected)

Start work with `/metaswarm:start`. Check status with `/metaswarm:status`.

## CRITICAL RULES:

- Create beads for each task in task.md file in each module and close it once completed.
- Take the screenshot of the output and keep it in the same folder for each module.
- DO NOT Commit any changes into Git
- Pause on gatepath and wait for the input to move forward
- Strictly follow the metaswarm. DO NOT stumbling to Claude.
- Make sure to mark the tick mark for each task within the Task.md file
- Make sure all the tasks are completed before mark module as completed.
- Keep the checklist for each task mentioned in task.md before ending the module session.
- DO NOT skip any task/steps mentioned in the task.md file.


<!-- BEGIN BEADS INTEGRATION v:1 profile:minimal hash:7510c1e2 -->
## Beads Issue Tracker

This project uses **bd (beads)** for issue tracking. Run `bd prime` to see full workflow context and commands.

### Quick Reference

```bash
bd ready              # Find available work
bd show <id>          # View issue details
bd update <id> --claim  # Claim work
bd close <id>         # Complete work
```

### Rules

- Use `bd` for ALL task tracking — do NOT use TodoWrite, TaskCreate, or markdown TODO lists
- Run `bd prime` for detailed command reference and session close protocol
- Use `bd remember` for persistent knowledge — do NOT use MEMORY.md files

**Architecture in one line:** issues live in a local Dolt DB; sync uses `refs/dolt/data` on your git remote; `.beads/issues.jsonl` is a passive export. See https://github.com/gastownhall/beads/blob/main/docs/SYNC_CONCEPTS.md for details and anti-patterns.

## Session Completion

**When ending a work session**, you MUST complete ALL steps below.

**MANDATORY WORKFLOW:**

1. **File issues for remaining work** - Create issues for anything that needs follow-up
2. **Run quality gates** (if code changed) - Tests, linters, builds
3. **Update issue status** - Close finished work, update in-progress items
4. **DO NOT PUSH TO REMOTE** - This is MANDATORY:
5. **Clean up** - Clear stashes, prune remote branches
6. **Verify** - All changes committed AND pushed
7. **Hand off** - Provide context for next session

**CRITICAL RULES:**
- NEVER Push the changes.
- Once everything completed, we will review and push at our end.
<!-- END BEADS INTEGRATION -->
