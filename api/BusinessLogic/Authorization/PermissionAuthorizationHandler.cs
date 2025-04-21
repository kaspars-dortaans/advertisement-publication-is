using BusinessLogic.Entities;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;

namespace BusinessLogic.Authorization;

public class PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory) : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

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
