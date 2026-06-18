using CRM.Application.Features.Leads.CreateLead;
using CRM.Domain.Enums;
using FluentValidation.Results;

namespace CRM.Tests.Features.Leads;

[TestFixture]
public class CreateLeadValidatorTests
{
    private CreateLeadValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateLeadValidator();
    }

    private static CreateLeadCommand ValidCommand() =>
        new("Alice", null, "ACME Corp", "alice@acme.com", null, LeadStatus.New, null);

    [Test]
    public async Task Validate_ValidCommand_Passes()
    {
        ValidationResult result = await _validator.ValidateAsync(ValidCommand());
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task Validate_EmptyFirstName_Fails()
    {
        var cmd = ValidCommand() with { FirstName = "" };
        ValidationResult result = await _validator.ValidateAsync(cmd);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<ValidationFailure>(
            e => e.PropertyName == nameof(CreateLeadCommand.FirstName)));
    }

    [Test]
    public async Task Validate_EmptyCompanyName_Fails()
    {
        var cmd = ValidCommand() with { CompanyName = "" };
        ValidationResult result = await _validator.ValidateAsync(cmd);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<ValidationFailure>(
            e => e.PropertyName == nameof(CreateLeadCommand.CompanyName)));
    }

    [Test]
    public async Task Validate_EmptyEmail_Fails()
    {
        var cmd = ValidCommand() with { Email = "" };
        ValidationResult result = await _validator.ValidateAsync(cmd);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<ValidationFailure>(
            e => e.PropertyName == nameof(CreateLeadCommand.Email)));
    }

    [Test]
    public async Task Validate_InvalidEmailFormat_Fails()
    {
        var cmd = ValidCommand() with { Email = "not-an-email" };
        ValidationResult result = await _validator.ValidateAsync(cmd);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<ValidationFailure>(
            e => e.PropertyName == nameof(CreateLeadCommand.Email)));
    }

    [Test]
    public async Task Validate_AllFieldsEmpty_FailsMultipleRules()
    {
        var cmd = ValidCommand() with { FirstName = "", CompanyName = "", Email = "" };
        ValidationResult result = await _validator.ValidateAsync(cmd);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.GreaterThanOrEqualTo(3));
    }

    [Test]
    public async Task Validate_WithOptionalFieldsNull_Passes()
    {
        var cmd = new CreateLeadCommand("Bob", null, "Beta Ltd", "bob@beta.com", null, LeadStatus.Contacted, null);
        ValidationResult result = await _validator.ValidateAsync(cmd);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task Validate_WithAllFieldsPopulated_Passes()
    {
        var ownerId = Guid.NewGuid();
        var cmd = new CreateLeadCommand("Alice", "Smith", "ACME Corp", "alice@acme.com", "+1-555-0100", LeadStatus.Qualified, ownerId);
        ValidationResult result = await _validator.ValidateAsync(cmd);
        Assert.That(result.IsValid, Is.True);
    }
}
