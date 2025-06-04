using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Permission;
using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public interface IPermissionService : IBaseService<Permission>
{
    public Task<DataTableQueryResponse<PermissionListItem>> GetPermissionList(DataTableQuery request);
}
