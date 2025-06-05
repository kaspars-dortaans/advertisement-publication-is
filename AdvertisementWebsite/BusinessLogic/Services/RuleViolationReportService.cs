using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.RuleViolationReport;
using BusinessLogic.Entities;
using BusinessLogic.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

public class RuleViolationReportService(Context dbContext) : BaseService<RuleViolationReport>(dbContext), IRuleViolationReportService
{
    public IQueryable<RuleViolationReportListItem> SelectListItem(int? limitDescriptionLength = null)
    {
        return DbSet.Select(r => new RuleViolationReportListItem
        {
            Id = r.Id,
            IsTrue = r.IsTrue,
            IsResolved = r.IsTrue != null,
            ResolutionDescription = limitDescriptionLength != null 
                ? (r.ResolutionDescription != null 
                    ? r.ResolutionDescription.Substring(0, Math.Min(r.ResolutionDescription.Length, limitDescriptionLength.Value)) 
                    : null)
                : r.ResolutionDescription,
            AdvertisementTitle = r.ReportedAdvertisement.Title,
            AdvertisementId = r.ReportedAdvertisementId,
            AdvertisementOwnerUsername = r.ReportedAdvertisement.Owner.UserName!,
            AdvertisementOwnerId = r.ReportedAdvertisement.OwnerId,
            ReporterUsername = r.Reporter != null ? r.Reporter.UserName : null,
            ReporterId = r.ReporterId,
            Description = limitDescriptionLength != null 
                ? r.Description.Substring(0, Math.Min(r.Description.Length, limitDescriptionLength.Value))
                : r.Description,
            ReportDate = r.ReportDate
        });
    }

    public Task<DataTableQueryResponse<RuleViolationReportListItem>> GetReportList(DataTableQuery request)
    {
        return DataTableQueryResolver.ResolveDataTableQuery(SelectListItem(100), request);
    }

    public Task ResolveReport(int id, bool isTrue, string resolutionDescription)
    {
        return DbSet.Where(r => r.Id == id).ExecuteUpdateAsync(setters => setters
            .SetProperty(r => r.IsTrue, r => isTrue)
            .SetProperty(r => r.ResolutionDescription, r => resolutionDescription));
    }
}
