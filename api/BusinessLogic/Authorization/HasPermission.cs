using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BusinessLogic.Authorization;

public class HasPermission : AuthorizeAttribute
{
    public HasPermission(Permissions permission) : base(permission.ToString()) {
        AuthenticationSchemes = IdentityConstants.BearerScheme;
    }
}
