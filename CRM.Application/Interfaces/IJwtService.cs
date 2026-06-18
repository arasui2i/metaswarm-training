namespace CRM.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, string[] roles, bool rememberMe);
}
