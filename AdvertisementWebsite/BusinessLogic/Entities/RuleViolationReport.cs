namespace BusinessLogic.Entities;

public class RuleViolationReport
{
    public int Id { get; set; }
    public string Description { get; set; } = default!;
    public DateTime ReportDate { get; set; }
    public int? ReporterId { get; set; }
    public int ReportedAdvertisementId { get; set; }
    public User? Reporter { get; set; }
    public Advertisement ReportedAdvertisement { get; set; } = default!;
}
