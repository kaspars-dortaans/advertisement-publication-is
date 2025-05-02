using BusinessLogic.Enums;

namespace BusinessLogic.Entities;

public class Cost
{
    public int Id { get; set; }
    public CostType Type { get; set; }
    public decimal Amount { get; set; }
}
