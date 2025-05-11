namespace BusinessLogic.Dto.Roles;

public class RoleListItem
{
    public int Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int PermissionCount { get; set; }
}
