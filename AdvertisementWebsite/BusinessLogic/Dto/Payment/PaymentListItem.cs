namespace BusinessLogic.Dto.Payment;

public class PaymentListItem
{
    public int Id { get; set; }
    public string? PayerUsername { get; set; }
    public decimal Amount { get; set; }
    public int PayerId { get; set; }
    public DateTime Date { get; set; }
    public int PaymentItemCount { get; set; }
}
