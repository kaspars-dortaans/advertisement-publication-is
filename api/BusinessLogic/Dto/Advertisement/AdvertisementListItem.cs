﻿namespace BusinessLogic.Dto.Advertisement;

public class AdvertisementListItem
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public DateTime PostedDate { get; set; }
    public string Title { get; set; } = default!;
    public string AdvertisementText { get; set; } = default!;
    public string? ThumbnailImagePath { get; set; }
    public IEnumerable<AttributeValueItem> AttributeValues { get; set; } = default!;
}

public class AttributeValueItem
{
    public int AttributeId { get; set; }
    public string AttributeName { get; set; } = default!;
    public string Value { get; set; } = default!;
    //For value list entry name
    public string? ValueName { get; set; } = default!;
}

