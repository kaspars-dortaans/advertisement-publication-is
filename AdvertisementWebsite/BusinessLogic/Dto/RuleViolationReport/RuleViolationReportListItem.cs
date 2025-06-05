namespace BusinessLogic.Dto.RuleViolationReport;

public class RuleViolationReportListItem
{
    public int Id { get; set; }
    public string Description { get; set; } = default!;
    public string? ResolutionDescription { get; set; }
    public string? ReporterUsername { get; set; }
    public int? ReporterId { get; set; }
    public string AdvertisementOwnerUsername { get; set; } = default!;
    public int AdvertisementOwnerId { get; set; }
    public string AdvertisementTitle { get; set; } = default!;
    public int AdvertisementId { get; set; }
    public DateTime ReportDate { get; set; }
    public bool? IsTrue { get; set; }
    public bool IsResolved { get; set; }
}
