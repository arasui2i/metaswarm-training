using CRM.Application.Features.Auth.Login;
using FluentValidation.Results;

namespace CRM.Tests.Auth;

[TestFixture]
public class LoginCommandValidatorTests
{
    private LoginCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new LoginCommandValidator();
    }

    [Test]
    public async Task Validate_ValidEmailAndPassword_Passes()
    {
        var command = new LoginCommand("user@example.com", "password123", false);
        ValidationResult result = await _validator.ValidateAsync(command);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task Validate_ValidUsernameAndPassword_Passes()
    {
        var command = new LoginCommand("johndoe", "securepass", false);
        ValidationResult result = await _validator.ValidateAsync(command);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task Validate_EmptyEmailOrUsername_Fails()
    {
        var command = new LoginCommand("", "password123", false);
        ValidationResult result = await _validator.ValidateAsync(command);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<ValidationFailure>(
            e => e.PropertyName == nameof(LoginCommand.EmailOrUsername)));
    }

    [Test]
    public async Task Validate_EmptyPassword_Fails()
    {
        var command = new LoginCommand("user@example.com", "", false);
        ValidationResult result = await _validator.ValidateAsync(command);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<ValidationFailure>(
            e => e.PropertyName == nameof(LoginCommand.Password)));
    }

    [Test]
    public async Task Validate_PasswordBelowMinimumLength_Fails()
    {
        var command = new LoginCommand("user@example.com", "12345", false);
        ValidationResult result = await _validator.ValidateAsync(command);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<ValidationFailure>(
            e => e.PropertyName == nameof(LoginCommand.Password)));
    }

    [Test]
    public async Task Validate_PasswordAtMinimumLength_Passes()
    {
        var command = new LoginCommand("user@example.com", "123456", false);
        ValidationResult result = await _validator.ValidateAsync(command);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task Validate_BothFieldsEmpty_FailsBothRules()
    {
        var command = new LoginCommand("", "", false);
        ValidationResult result = await _validator.ValidateAsync(command);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.GreaterThanOrEqualTo(2));
    }
}
