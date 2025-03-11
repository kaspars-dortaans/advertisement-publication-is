using Microsoft.AspNetCore.Identity;

namespace BusinessLogic.Entities;

public class Role : IdentityRole<int>
{
    public ICollection<IdentityUserRole<int>> IdentityUserRoles { get; set; } = [];
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<Permission> Permissions { get; set; } = [];
}
