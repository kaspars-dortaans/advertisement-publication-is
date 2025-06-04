using BusinessLogic.Constants;
using BusinessLogic.Enums;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Payments;

public class SetServicePricesRequest
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public Dictionary<CostType, decimal> Prices { get; set; } = default!;
}
