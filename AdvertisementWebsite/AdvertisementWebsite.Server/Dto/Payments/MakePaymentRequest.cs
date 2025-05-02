using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Payments;

public class MakePaymentRequest
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public IEnumerable<NewPaymentItem> PaymentItems { get; set; } = default!;

    /// <summary>
    /// Total amount user has agreed to pay, use only for comparison!
    /// </summary>
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public decimal TotalAmountConfirmation { get; set; }
}
