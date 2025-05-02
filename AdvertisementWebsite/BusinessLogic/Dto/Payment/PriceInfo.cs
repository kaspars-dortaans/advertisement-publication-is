namespace BusinessLogic.Dto.Payment;

public class PriceInfo
{
    public IEnumerable<PaymentItemDto> Items { get; set; } = default!;
    public decimal TotalAmount { get; set; }
}
