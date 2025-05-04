namespace BusinessLogic.Dto.Payment;

public class PaymentDataTableQuery : DataTableQuery.DataTableQuery
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
