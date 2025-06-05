using AdvertisementWebsite.Server.Dto.Advertisement;
using AdvertisementWebsite.Server.Dto.RuleViolationReport;
using AutoMapper;
using BusinessLogic.Authorization;
using BusinessLogic.Constants;
using BusinessLogic.Dto;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.RuleViolationReport;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementWebsite.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class RuleViolationReportController (
    IMapper mapper,
    IRuleViolationReportService ruleViolationReportService
    ) : ControllerBase
{
    private readonly IMapper _mapper = mapper;
    private readonly IRuleViolationReportService _ruleViolationReportService = ruleViolationReportService;

    [AllowAnonymous]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task ReportAdvertisement(ReportAdvertisementRequest request)
    {
        var report = _mapper.Map<RuleViolationReport>(request, o => o.Items[nameof(User)] = User);
        await _ruleViolationReportService.AddAsync(report);
    }

    [HasPermission(Permissions.ViewRuleViolationReports)]
    [ProducesResponseType<DataTableQueryResponse<RuleViolationReportListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<DataTableQueryResponse<RuleViolationReportListItem>> GetReportList(DataTableQuery request)
    {
        return await _ruleViolationReportService.GetReportList(request);
    }

    [HasPermission(Permissions.ViewRuleViolationReports)]
    [ProducesResponseType<RuleViolationReportListItem>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<RuleViolationReportListItem> GetReport(int id)
    {
        return (await _ruleViolationReportService
            .SelectListItem()
            .FirstOrDefaultAsync(r => r.Id == id))
            ?? throw new ApiException([CustomErrorCodes.NotFound]);
    }

    [HasPermission(Permissions.ResolveRuleViolationReport)]
    [ProducesResponseType<Ok>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task ResolveReport(ResolveRuleViolationReportRequest request)
    {
        await _ruleViolationReportService.ResolveReport(request.Id, request.IsTrue, request.ResolutionDescription);
    }
}
