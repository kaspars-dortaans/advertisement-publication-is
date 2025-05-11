using BusinessLogic.Entities;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace BusinessLogic.Authorization;

public class AnyOfPermissionsAuthorizationHandler(IServiceScopeFactory serviceScopeFactory) : AuthorizationHandler<AnyOfPermissionsRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AnyOfPermissionsRequirement requirement)
    {
        var userIdString = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userIdString is null || !int.TryParse(userIdString, out var userId))
        {
            return Task.CompletedTask;
        }

        var scope = _serviceScopeFactory.CreateScope();
        var rolePermissionService = scope.ServiceProvider.GetRequiredService<IBaseService<RolePermission>>();

        //Check if any of this user roles has required permission
        if (rolePermissionService.Exists(rp =>
            rp.Role.IdentityUserRoles.Any(ur => ur.UserId == userId)
            && requirement.Permissions.Any(permission => permission == rp.Permission.Name)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;

    }
}

