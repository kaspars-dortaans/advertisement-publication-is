namespace AdvertisementWebsite.Server.Dto.Roles;

public class RoleFormRequest
{
    public int? Id { get; set; }
    public string Name { get; set; } = default!;
    public IEnumerable<int> Permissions { get; set; } = default!;
}
