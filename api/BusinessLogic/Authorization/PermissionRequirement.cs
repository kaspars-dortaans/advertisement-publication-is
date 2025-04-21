using Microsoft.AspNetCore.Authorization;

namespace BusinessLogic.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
