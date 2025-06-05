using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.RuleViolationReport;
using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public interface IRuleViolationReportService : IBaseService<RuleViolationReport>
{
    public IQueryable<RuleViolationReportListItem> SelectListItem(int? limitDescriptionLength = null);
    public Task<DataTableQueryResponse<RuleViolationReportListItem>> GetReportList(DataTableQuery request);
    public Task ResolveReport(int id, bool isTrue, string resolutionDescription);
}
