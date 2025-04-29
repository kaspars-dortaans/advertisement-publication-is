using BusinessLogic.Dto.Advertisement;

namespace AdvertisementWebsite.Server.Dto.Advertisement;

public class AdvertisementFormInfo
{
    public CreateOrEditAdvertisementRequest Advertisement { get; set; } = default!;
    public CategoryFormInfo CategoryInfo { get; set; } = default!;
}
