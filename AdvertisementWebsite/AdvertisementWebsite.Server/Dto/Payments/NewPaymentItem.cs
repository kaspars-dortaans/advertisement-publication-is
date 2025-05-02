using BusinessLogic.Constants;
using BusinessLogic.Dto.Time;
using BusinessLogic.Enums;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Payments;

public class NewPaymentItem
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public int PaymentSubjectId { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public PaymentType Type { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public PostTimeDto TimePeriod { get; set; } = default!;
}
