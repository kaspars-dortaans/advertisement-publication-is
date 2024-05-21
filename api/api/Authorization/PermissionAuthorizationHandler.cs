using api.Entities;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace api.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdString = context.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        if (userIdString is null || !int.TryParse(userIdString, out var userId))
        {
            return Task.CompletedTask;
        }

        var scope = _serviceScopeFactory.CreateScope();
        var rolePermissionService = scope.ServiceProvider.GetRequiredService<IBaseService<RolePermission>>();

        //Check if any of this user roles has required permission
        if (rolePermissionService.Exists(rp => 
            rp.Permission.Name == requirement.Permission 
            && rp.Role.IdentityUserRoles.Any(ur => ur.UserId == userId)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;

    }
}
