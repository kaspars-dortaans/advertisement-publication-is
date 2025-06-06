﻿using AdvertisementWebsite.Server.Dto.Payments;
using AdvertisementWebsite.Server.Helpers;
using AutoMapper;
using BusinessLogic.Authorization;
using BusinessLogic.Dto;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Payment;
using BusinessLogic.Enums;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementWebsite.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class PaymentController(
    IPaymentService paymentService,
    IMapper mapper
    ) : ControllerBase
{
    private readonly IPaymentService _paymentService = paymentService;
    private readonly IMapper _mapper = mapper;

    [HasPermission(Permissions.ViewOwnPayments)]
    [ProducesResponseType<DataTableQueryResponse<PaymentListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<DataTableQueryResponse<PaymentListItem>> GetUserPayments(PaymentDataTableQuery query)
    {
        return await _paymentService.GetPayments(query, User.GetUserId()!.Value);
    }

    [HasPermission(Permissions.ViewSystemPayments)]
    [ProducesResponseType<DataTableQueryResponse<PaymentListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<DataTableQueryResponse<PaymentListItem>> GetAllPayments(PaymentDataTableQuery query)
    {
        return await _paymentService.GetPayments(query);
    }

    [HasPermission(Permissions.ViewOwnPayments)]
    [ProducesResponseType<PriceInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<PriceInfo> GetUserPayment(int paymentId)
    {
        return await _paymentService.GetPriceInfo(paymentId, User.GetUserId()!.Value);
    }

    [HasPermission(Permissions.ViewSystemPayments)]
    [ProducesResponseType<PriceInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<PriceInfo> GeSystemPayment(int paymentId)
    {
        return await _paymentService.GetPriceInfo(paymentId);
    }

    [HasPermission(Permissions.MakePayment)]
    [ProducesResponseType<PriceInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<PriceInfo> CalculatePrices(IEnumerable<NewPaymentItem> items)
    {
        var dto = _mapper.Map<IEnumerable<PaymentItemDto>>(items);
        return await _paymentService.GetPriceInfo(dto, User.GetUserId()!.Value);
    }

    [HasPermission(Permissions.MakePayment)]
    [ProducesResponseType<Ok>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task MakePayment(MakePaymentRequest request)
    {
        var items = _mapper.Map<IEnumerable<PaymentItemDto>>(request.PaymentItems);
        await _paymentService.MakePayment(items, request.TotalAmountConfirmation, User.GetUserId()!.Value);
    }

    [HasPermission(Permissions.ManageServicePrices)]
    [ProducesResponseType<Dictionary<CostType, decimal>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<Dictionary<CostType, decimal>> GetServicePrices()
    {
        return await _paymentService.GetServicePrices();
    }

    [HasPermission(Permissions.ManageServicePrices)]
    [ProducesResponseType<Ok>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task SetServicePrices(SetServicePricesRequest request)
    {
        await _paymentService.SetServicePrices(request.Prices);
    }
}
