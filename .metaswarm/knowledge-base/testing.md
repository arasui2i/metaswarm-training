---
name: crm-testing
description: Testing conventions, coverage requirements, and example patterns for CRM
metadata:
  type: testing
---

# Testing Conventions

## Coverage Threshold

**80% line + branch** — enforced on CI for both backend and frontend.

## Backend (nUnit)

Location: `CRM.Tests/` (mirror solution structure)

Key test classes per feature:
- `{Action}CommandHandlerTests` — happy path, error paths (invalid user, wrong password, etc.)
- `{Action}CommandValidatorTests` — empty fields, format errors

Pattern:
```csharp
[TestFixture]
public class LoginCommandHandlerTests
{
    private Mock<IUserRepository> _userRepo;
    private Mock<IJwtService> _jwtService;
    private LoginCommandHandler _handler;

    [SetUp]
    public void SetUp() { /* init mocks */ }

    [Test]
    public async Task Handle_ValidCredentials_ReturnsToken() { ... }

    [Test]
    public async Task Handle_WrongPassword_ThrowsUnauthorized() { ... }
}
```

## Frontend (Vitest + React Testing Library)

Location: `src/__tests__/` or co-located `*.test.tsx`

Key tests per feature:
- Page component — renders fields, shows validation errors, submits correctly
- Hook — success path stores token + navigates; error path surfaces message
- Route guards — unauthenticated redirect

Pattern:
```tsx
describe('LoginPage', () => {
  it('renders email and password fields', () => { ... });
  it('shows error when fields are empty', async () => { ... });
  it('calls loginApi and navigates to /customers on success', async () => { ... });
});
```

## CI Gate

Backend: `dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings` → Coverlet XML → 80% line/branch
- `coverlet.runsettings` excludes `[CRM.Infrastructure]CRM.Infrastructure.Persistence.*`, `Repositories.*`, `Migrations.*` (EF/DB-dependent, not unit-testable)

Frontend: `vitest run --coverage` → v8 provider → 80% line/branch
- Coverage exclude in `vite.config.ts`: `src/App.tsx`, `src/api/**`, stub pages (not unit-testable at this layer)
- Tests at `src/src/__tests__/` — NOT `src/__tests__/` (note nested Vite source root)

## Frontend Gotchas (login module learnings)

1. **Vite source root is nested**: All source files live in `src/src/` (not `src/`). The npm project is at `src/`, Vite source root is `src/src/`. Test files go in `src/src/__tests__/`.

2. **Import path from pages to hooks**: From `src/src/pages/Login/LoginPage.tsx` to `src/src/hooks/`, the correct path is `../../hooks/useLogin` (two levels up), NOT `../hooks/useLogin`.

3. **MUI v9 uses `slotProps` not `InputProps`**: `TextField` adornments must use `slotProps={{ input: { endAdornment: ... } }}`. The old `InputProps` prop is silently ignored in MUI v9.

4. **`vi.mock` hoisting — TDZ trap**: `vi.mock()` is hoisted above all code. If the factory references an outer `const mockFn = vi.fn()`, that `const` is in the temporal dead zone when the factory runs. Fix: use `vi.fn()` directly inside the factory, then access via `vi.mocked(importedFn)` after the import.

5. **RTL `getByLabelText` with password toggle**: When `IconButton` has `aria-label="Show password"`, the regex `/password/i` matches BOTH the field label and the button. Use exact string `getByLabelText('Password')` instead.

6. **React Query mutation tests**: Use `renderHook` with `QueryClientProvider` wrapper, trigger with `result.current.mutate(payload)`, then `await waitFor(() => expect(result.current.isSuccess).toBe(true))`.

## Backend Gotchas (login module learnings)

1. **BCrypt package**: Add `BCrypt.Net-Next` to `CRM.Application.csproj` (not just Infrastructure) if the handler calls `BCrypt.Net.BCrypt.Verify()` in the Application layer.

2. **IConfiguration mock**: Use `Mock<IConfiguration>` with `mockConfig.Setup(x => x["Jwt:Key"]).Returns(...)` — the indexer syntax works fine in Moq.

3. **EF Design-time factory**: `AppDbContextFactory` implements `IDesignTimeDbContextFactory<AppDbContext>` and reads from `CRM.API/appsettings.json` — required for `dotnet ef migrations add` from the Infrastructure project.
