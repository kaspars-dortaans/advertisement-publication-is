using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Dto.Advertisement;

public class AdvertisementQuery : DataTableQuery.DataTableQuery
{
    public int? CategoryId { get; set; }
    public int? AdvertisementOwnerId { get; set; }
    public IEnumerable<int>? AdvertisementIds { get; set; }
    public IEnumerable<AttributeSearchQuery> AttributeSearch { get; set; } = default!;
    public IEnumerable<AttributeOrderQuery> AttributeOrder { get; set; } = default!;

}

public class AttributeSearchQuery
{
    [Required]
    public int AttributeId { get; set; }
    public string? Value { get; set; } = default!;
    public string? SecondaryValue { get; set; } = default!;
}

public class AttributeOrderQuery
{
    [Required]
    public int AttributeId { get; set; }
    [Required]
    public string Direction { get; set; } = default!;
}