using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.RuleViolationReport;

public class ResolveRuleViolationReportRequest
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public int Id { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public bool IsTrue { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string ResolutionDescription { get; set; } = default!;
}
