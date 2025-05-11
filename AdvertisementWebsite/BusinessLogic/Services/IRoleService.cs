using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Roles;
using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public interface IRoleService : IBaseService<Role>
{
    public Task<DataTableQueryResponse<RoleListItem>> GetRoles(DataTableQuery request);
    public Task UpdateRole(Role roleToUpdate);
}
