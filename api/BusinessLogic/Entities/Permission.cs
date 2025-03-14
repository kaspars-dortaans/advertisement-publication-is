namespace BusinessLogic.Entities;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public ICollection<Role> AddedToRoles { get; set; } = [];

}
