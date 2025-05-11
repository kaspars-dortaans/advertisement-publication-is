using BusinessLogic.Constants;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Roles;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

public class RoleService(Context dbContext) : BaseService<Role>(dbContext), IRoleService
{
    public Task<DataTableQueryResponse<RoleListItem>> GetRoles(DataTableQuery request)
    {
        var query = GetAll()
            .Select(r => new RoleListItem
            {
                Id = r.Id,
                Name = r.Name!,
                PermissionCount = r.Permissions.Count()
            });
        return DataTableQueryResolver.ResolveDataTableQuery(query, request);
    }

    public async Task UpdateRole (Role roleToUpdate)
    {
        var existingRole = (await GetAll()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleToUpdate.Id))
            ?? throw new ApiException([CustomErrorCodes.NotFound]);

        existingRole.Name = roleToUpdate.Name;
        existingRole.NormalizedName = roleToUpdate.NormalizedName;

        //Remove
        foreach (var existingRp in existingRole.RolePermissions)
        {
            if (!roleToUpdate.RolePermissions.Any(rp => rp.PermissionId == existingRp.PermissionId))
            {
                existingRole.RolePermissions.Remove(existingRp);
            }
        }

        //Add
        foreach(var newRp in roleToUpdate.RolePermissions)
        {
            if(!existingRole.RolePermissions.Any(p => p.PermissionId == newRp.PermissionId))
            {
                existingRole.RolePermissions.Add(newRp);
            }
        }

        await DbContext.SaveChangesAsync();
    }
}
