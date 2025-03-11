using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace BusinessLogic.Authorization;

public class HasPermission : AuthorizeAttribute
{
    public HasPermission(Permissions permission) : base(permission.ToString()) {
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
    }
}
