using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Permission;
using BusinessLogic.Entities;
using BusinessLogic.Helpers;

namespace BusinessLogic.Services;

public class PermissionService(Context dbContext) : BaseService<Permission>(dbContext), IPermissionService
{
    public Task<DataTableQueryResponse<PermissionListItem>> GetPermissionList(DataTableQuery request)
    {
        var query = DbSet.Select(p => new PermissionListItem
        {
            Id = p.Id,
            Name = p.Name,
        });
        return DataTableQueryResolver.ResolveDataTableQuery(query, request);
    }
}
