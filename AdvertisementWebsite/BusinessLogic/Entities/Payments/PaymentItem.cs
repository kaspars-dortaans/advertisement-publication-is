using BusinessLogic.Enums;

namespace BusinessLogic.Entities.Payments;

public class PaymentItem
{
    public int Id { get; set; }
    public int PaymentId { get; set; }

    /// <summary>
    /// Advertisement or subscription id
    /// </summary>
    public int PaymentSubjectId { get; set; }
    public PaymentType Type { get; set; }
    public decimal Price { get; set; }
    public string Title { get; set; } = default!;
    public int TimePeriodInDays { get; set; }

    public Payment Payment { get; set; } = default!;
}
