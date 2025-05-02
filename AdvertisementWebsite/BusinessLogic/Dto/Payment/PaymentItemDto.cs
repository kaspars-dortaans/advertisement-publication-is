using BusinessLogic.Dto.Time;
using BusinessLogic.Enums;

namespace BusinessLogic.Dto.Payment;

public class PaymentItemDto
{
    public int PaymentSubjectId { get; set; }
    public PaymentType Type { get; set; }
    public string Title { get; set; } = default!;
    public decimal Price { get; set; }
    public PostTimeDto TimePeriod { get; set; } = default!;
}
