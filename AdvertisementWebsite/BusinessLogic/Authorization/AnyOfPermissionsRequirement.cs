using Microsoft.AspNetCore.Authorization;

namespace BusinessLogic.Authorization;

public class AnyOfPermissionsRequirement(string permission) : IAuthorizationRequirement
{
    public ICollection<string> Permissions { get; } = permission.Split(',');
}
