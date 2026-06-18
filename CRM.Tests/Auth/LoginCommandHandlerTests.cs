using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Auth.Login;
using CRM.Application.Interfaces;
using CRM.Domain.Entities;
using Moq;

namespace CRM.Tests.Auth;

[TestFixture]
public class LoginCommandHandlerTests
{
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<IJwtService> _jwtServiceMock = null!;
    private LoginCommandHandler _handler = null!;
    private User _testUser = null!;
    private const string RawPassword = "password123";

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _handler = new LoginCommandHandler(_userRepositoryMock.Object, _jwtServiceMock.Object);

        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(RawPassword),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _jwtServiceMock
            .Setup(s => s.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
            .Returns("mock-jwt-token");
    }

    [Test]
    public async Task Handle_ValidEmailLogin_ReturnsTokenAndUser()
    {
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(_testUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        _userRepositoryMock
            .Setup(r => r.GetWithRolesAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        var command = new LoginCommand(_testUser.Email, RawPassword, false);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.AccessToken, Is.EqualTo("mock-jwt-token"));
        Assert.That(result.User.Email, Is.EqualTo(_testUser.Email));
        Assert.That(result.User.Username, Is.EqualTo(_testUser.Username));
    }

    [Test]
    public async Task Handle_ValidUsernameLogin_ReturnsTokenAndUser()
    {
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(_testUser.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepositoryMock
            .Setup(r => r.GetByUsernameAsync(_testUser.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        _userRepositoryMock
            .Setup(r => r.GetWithRolesAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        var command = new LoginCommand(_testUser.Username, RawPassword, false);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.User.Id, Is.EqualTo(_testUser.Id));
    }

    [Test]
    public void Handle_UserNotFound_ThrowsUnauthorizedException()
    {
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepositoryMock
            .Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new LoginCommand("nobody@example.com", RawPassword, false);

        Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public void Handle_WrongPassword_ThrowsUnauthorizedException()
    {
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(_testUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        var command = new LoginCommand(_testUser.Email, "wrongpassword", false);

        Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_RememberMeTrue_ExpiresInSevenDays()
    {
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(_testUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        _userRepositoryMock
            .Setup(r => r.GetWithRolesAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        var command = new LoginCommand(_testUser.Email, RawPassword, true);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.ExpiresAt, Is.EqualTo(DateTime.UtcNow.AddDays(7)).Within(TimeSpan.FromSeconds(10)));
    }

    [Test]
    public async Task Handle_RememberMeFalse_ExpiresInOneDay()
    {
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(_testUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        _userRepositoryMock
            .Setup(r => r.GetWithRolesAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        var command = new LoginCommand(_testUser.Email, RawPassword, false);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.ExpiresAt, Is.EqualTo(DateTime.UtcNow.AddDays(1)).Within(TimeSpan.FromSeconds(10)));
    }

    [Test]
    public async Task Handle_WithRoles_RolesIncludedInResult()
    {
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };
        _testUser.UserRoles = new List<UserRole>
        {
            new() { UserId = _testUser.Id, RoleId = adminRole.Id, Role = adminRole }
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(_testUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        _userRepositoryMock
            .Setup(r => r.GetWithRolesAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        var command = new LoginCommand(_testUser.Email, RawPassword, false);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.User.Roles, Contains.Item("Admin"));
    }

    [Test]
    public async Task Handle_NoRoles_ReturnsEmptyRolesArray()
    {
        _testUser.UserRoles = new List<UserRole>();

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(_testUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        _userRepositoryMock
            .Setup(r => r.GetWithRolesAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        var command = new LoginCommand(_testUser.Email, RawPassword, false);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.User.Roles, Is.Empty);
    }
}
