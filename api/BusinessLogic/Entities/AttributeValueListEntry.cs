﻿namespace BusinessLogic.Entities;

public class AttributeValueListEntry
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public int AttributeValueListId { get; set; }

    public AttributeValueList AttributeValueList { get; set; } = default!;
}
