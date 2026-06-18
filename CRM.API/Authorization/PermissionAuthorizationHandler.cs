using Microsoft.AspNetCore.Authorization;

namespace CRM.API.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasRole = context.User.Claims.Any(c =>
            c.Type == System.Security.Claims.ClaimTypes.Role &&
            c.Value == requirement.ActionKey);

        if (hasRole)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
