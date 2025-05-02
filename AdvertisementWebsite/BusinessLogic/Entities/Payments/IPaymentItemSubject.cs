namespace BusinessLogic.Entities.Payments;

public interface IPaymentItemSubject
{
    public int Id { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? ValidToDate { get; set; }
    public string Title { get; set; }
    public bool IsActive { get; set; }
}
