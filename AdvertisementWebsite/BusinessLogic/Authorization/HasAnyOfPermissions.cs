using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BusinessLogic.Authorization;

public class HasAnyOfPermissions : AuthorizeAttribute
{
    public HasAnyOfPermissions(Permissions[] permissions) : base(PermissionConstants.AnyOfPermissionsPrefix + string.Join(',', permissions.Select(p => p.ToString())))
    {
        AuthenticationSchemes = IdentityConstants.BearerScheme;
    }
}
