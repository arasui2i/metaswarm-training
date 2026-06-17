---
name: crm-testing
description: Testing conventions, coverage requirements, and example patterns for CRM
metadata:
  type: testing
---

# Testing Conventions

## Coverage Threshold

**80%** — enforced on CI for both backend and frontend.

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

Backend: `dotnet test --collect:"XPlat Code Coverage"` → Coverlet XML → 80% line coverage
Frontend: `vitest run --coverage` → Istanbul → 80% line/branch coverage
