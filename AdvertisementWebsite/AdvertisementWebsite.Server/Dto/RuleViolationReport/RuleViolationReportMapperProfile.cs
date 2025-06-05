using AdvertisementWebsite.Server.Dto.Advertisement;
using AdvertisementWebsite.Server.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdvertisementWebsite.Server.Dto.RuleViolationReport;

public class RuleViolationReportMapperProfile : Profile
{
    public RuleViolationReportMapperProfile()
    {
        CreateMap<ReportAdvertisementRequest, BusinessLogic.Entities.RuleViolationReport>()
            .ForMember(report => report.ReportDate, o => o.MapFrom(request => DateTime.UtcNow))
            .ForMember(report => report.ReporterId, o => o.MapFrom((request, _, _, context) =>
                (context.Items[nameof(ControllerBase.User)] as ClaimsPrincipal)?.GetUserId()));
    }
}
