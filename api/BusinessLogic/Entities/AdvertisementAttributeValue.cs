﻿namespace BusinessLogic.Entities;

public class AdvertisementAttributeValue
{
    public int Id {  get; set; }
    public string Value { get; set; } = default!;
    public int AdvertisementId { get; set; }
    public Advertisement Advertisement { get; set; } = default!;
    public int AttributeId { get; set; }
    public Attribute Attribute { get; set;} = default!;
}
