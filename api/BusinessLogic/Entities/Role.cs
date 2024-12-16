using Microsoft.AspNetCore.Identity;

namespace BusinessLogic.Entities;

public class Role : IdentityRole<int>
{
    public ICollection<IdentityUserRole<int>> IdentityUserRoles { get; set; } = new List<IdentityUserRole<int>>();
}
