using BCrypt.Net;
using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces;
using MediatR;

namespace CRM.Application.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginCommandResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<LoginCommandResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.EmailOrUsername, cancellationToken)
                   ?? await _userRepository.GetByUsernameAsync(request.EmailOrUsername, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException();

        var userWithRoles = await _userRepository.GetWithRolesAsync(user.Id, cancellationToken);
        var roles = userWithRoles?.UserRoles.Select(ur => ur.Role.Name).ToArray() ?? [];
        var permissions = userWithRoles?.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.ActionKey)
            .Distinct().ToArray() ?? [];

        var token = _jwtService.GenerateToken(user.Id, user.Email, roles, permissions, request.RememberMe);
        var expiresAt = request.RememberMe
            ? DateTime.UtcNow.AddDays(7)
            : DateTime.UtcNow.AddDays(1);

        return new LoginCommandResult(
            token,
            expiresAt,
            new LoginUserDto(user.Id, user.Email, user.Username, roles)
        );
    }
}
