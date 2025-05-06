using BusinessLogic.Dto.Category;

namespace AdvertisementWebsite.Server.Dto.Advertisement;

public class AdvertisementFormInfo
{
    public CreateOrEditAdvertisementRequest Advertisement { get; set; } = default!;
    public CategoryAttributeListData CategoryInfo { get; set; } = default!;
}
