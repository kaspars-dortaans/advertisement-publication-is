namespace BusinessLogic.Dto.Payment;

public class PriceInfo
{
    public string? PayerUsername { get; set; }
    public IEnumerable<PaymentItemDto> Items { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public DateTime? Date { get; set; }
}
