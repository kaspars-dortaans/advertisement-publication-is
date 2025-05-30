using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Roles;

public class RoleFormRequest
{
    public int? Id { get; set; }

    [MaxLength(InputConstants.MaxTitleLength)]
    public string Name { get; set; } = default!;
    public IEnumerable<int> Permissions { get; set; } = default!;
}
