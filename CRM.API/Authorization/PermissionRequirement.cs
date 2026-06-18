using Microsoft.AspNetCore.Authorization;

namespace CRM.API.Authorization;

public record PermissionRequirement(string ActionKey) : IAuthorizationRequirement;
