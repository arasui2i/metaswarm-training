using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CRM.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CRM.Tests.Auth;

[TestFixture]
public class JwtServiceTests
{
    private JwtService _sut = null!;
    private const string TestKey = "test-super-secret-key-32-chars!!";

    [SetUp]
    public void SetUp()
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x["Jwt:Key"]).Returns(TestKey);
        mockConfig.Setup(x => x["Jwt:Issuer"]).Returns("CRM.API");
        mockConfig.Setup(x => x["Jwt:Audience"]).Returns("CRM.Client");
        _sut = new JwtService(mockConfig.Object);
    }

    [Test]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateToken(Guid.NewGuid(), "user@example.com", [], false);
        Assert.That(token, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void GenerateToken_ContainsSubClaim_MatchingUserId()
    {
        var userId = Guid.NewGuid();
        var token = _sut.GenerateToken(userId, "user@example.com", [], false);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.That(parsed.Subject, Is.EqualTo(userId.ToString()));
    }

    [Test]
    public void GenerateToken_ContainsEmailClaim()
    {
        var email = "user@example.com";
        var token = _sut.GenerateToken(Guid.NewGuid(), email, [], false);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var emailClaim = parsed.Claims.FirstOrDefault(c =>
            c.Type == JwtRegisteredClaimNames.Email || c.Type == "email");

        Assert.That(emailClaim, Is.Not.Null);
        Assert.That(emailClaim!.Value, Is.EqualTo(email));
    }

    [Test]
    public void GenerateToken_ContainsRoleClaims()
    {
        var roles = new[] { "Admin", "Sales" };
        var token = _sut.GenerateToken(Guid.NewGuid(), "user@example.com", roles, false);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var roleClaims = parsed.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToArray();

        Assert.That(roleClaims, Has.Member("Admin"));
        Assert.That(roleClaims, Has.Member("Sales"));
    }

    [Test]
    public void GenerateToken_RememberMeTrue_ExpiresInSevenDays()
    {
        var token = _sut.GenerateToken(Guid.NewGuid(), "user@example.com", [], true);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.That(parsed.ValidTo, Is.EqualTo(DateTime.UtcNow.AddDays(7)).Within(TimeSpan.FromSeconds(10)));
    }

    [Test]
    public void GenerateToken_RememberMeFalse_ExpiresInOneDay()
    {
        var token = _sut.GenerateToken(Guid.NewGuid(), "user@example.com", [], false);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.That(parsed.ValidTo, Is.EqualTo(DateTime.UtcNow.AddDays(1)).Within(TimeSpan.FromSeconds(10)));
    }

    [Test]
    public void GenerateToken_SetsCorrectIssuer()
    {
        var token = _sut.GenerateToken(Guid.NewGuid(), "user@example.com", [], false);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.That(parsed.Issuer, Is.EqualTo("CRM.API"));
    }

    [Test]
    public void GenerateToken_SetsCorrectAudience()
    {
        var token = _sut.GenerateToken(Guid.NewGuid(), "user@example.com", [], false);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.That(parsed.Audiences, Has.Member("CRM.Client"));
    }

    [Test]
    public void GenerateToken_NoRoles_ProducesNoRoleClaims()
    {
        var token = _sut.GenerateToken(Guid.NewGuid(), "user@example.com", [], false);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var roleClaims = parsed.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .ToArray();

        Assert.That(roleClaims, Is.Empty);
    }

    [Test]
    public void GenerateToken_ContainsJtiClaim()
    {
        var token = _sut.GenerateToken(Guid.NewGuid(), "user@example.com", [], false);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var jti = parsed.Claims.FirstOrDefault(c =>
            c.Type == JwtRegisteredClaimNames.Jti || c.Type == "jti");

        Assert.That(jti, Is.Not.Null);
        Assert.That(jti!.Value, Is.Not.Empty);
    }
}
