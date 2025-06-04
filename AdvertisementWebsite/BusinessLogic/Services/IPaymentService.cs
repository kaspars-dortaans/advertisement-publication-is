using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Payment;
using BusinessLogic.Entities.Payments;
using BusinessLogic.Enums;

namespace BusinessLogic.Services;

public interface IPaymentService : IBaseService<Payment>
{
    public Task<DataTableQueryResponse<PaymentListItem>> GetPayments(PaymentDataTableQuery query, int? userId = null);

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
    public Task<PriceInfo> GetPriceInfo(int paymentId, int? uerId = null);

    /// <summary>
    /// Confirm total amount is correct, and make payment
    /// </summary>
    /// <param name="items"></param>
    /// <param name="confirmTotalAmount"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task MakePayment(IEnumerable<PaymentItemDto> items, decimal confirmTotalAmount, int userId);

    public Task SetServicePrices(Dictionary<CostType, decimal> setPrices);

    public Task<Dictionary<CostType, decimal>> GetServicePrices();
}
