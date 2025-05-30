using AdvertisementWebsite.Server.Validators;
using BusinessLogic.Constants;
using BusinessLogic.Dto.Time;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.AdvertisementNotification;

public class CreateOrEditNotificationSubscriptionRequest
{
    public int? Id { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerUsername { get; set; }

    [MaxLength(InputConstants.MaxTitleLength, ErrorMessage = CustomErrorCodes.MaxLength)]
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Title { get; set; } = default!;

    [MaxLength(InputConstants.MaxSubscriptionKeywordCount, ErrorMessage = CustomErrorCodes.MaxLength)]
    [ElementLength(InputConstants.MaxSubscriptionKeywordLength)]
    public IEnumerable<string>? Keywords { get; set; }

    public PostTimeDto? PaidTime { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public int CategoryId { get; set; }

    public DateTime? ValidToDate { get; set; }

    public IEnumerable<KeyValuePair<int, string>> AttributeValues { get; set; } = default!;
}
