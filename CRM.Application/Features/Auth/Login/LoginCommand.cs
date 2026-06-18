using MediatR;

namespace CRM.Application.Features.Auth.Login;

public record LoginCommand(string EmailOrUsername, string Password, bool RememberMe)
    : IRequest<LoginCommandResult>;

public record LoginCommandResult(string AccessToken, DateTime ExpiresAt, LoginUserDto User);

public record LoginUserDto(Guid Id, string Email, string Username, string[] Roles);
