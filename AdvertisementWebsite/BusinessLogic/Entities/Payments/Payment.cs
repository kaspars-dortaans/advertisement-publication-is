namespace BusinessLogic.Entities.Payments;

public class Payment
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public int PayerId { get; set; }
    public User Payer { get; set; } = default!;
    public DateTime Date { get; set; }

    public ICollection<PaymentItem> Items { get; set; } = default!;
}
