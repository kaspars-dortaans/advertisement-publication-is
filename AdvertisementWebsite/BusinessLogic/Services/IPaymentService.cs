using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Payment;
using BusinessLogic.Entities.Payments;

namespace BusinessLogic.Services;

public interface IPaymentService : IBaseService<Payment>
{
    public Task<DataTableQueryResponse<PaymentListItem>> GetUserPayments(PaymentDataTableQuery query, int userId);

    /// <summary>
    /// Return dto with total amount, payment items with filled prices and titles
    /// </summary>
    /// <param name="items"></param>
    /// <param name="uerId"></param>
    /// <returns></returns>
    public Task<PriceInfo> GetPriceInfo(IEnumerable<PaymentItemDto> items, int uerId);

    /// <summary>
    /// Return dto with total amount and payment items
    /// </summary>
    /// <param name="items"></param>
    /// <param name="uerId"></param>
    /// <returns></returns>
    public Task<PriceInfo> GetPriceInfo(int paymentId, int uerId);

    /// <summary>
    /// Confirm total amount is correct, and make payment
    /// </summary>
    /// <param name="items"></param>
    /// <param name="confirmTotalAmount"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task MakePayment(IEnumerable<PaymentItemDto> items, decimal confirmTotalAmount, int userId);
}
