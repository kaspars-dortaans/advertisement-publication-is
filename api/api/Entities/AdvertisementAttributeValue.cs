﻿namespace api.Entities
{
    public class AdvertisementAttributeValue
    {
        public int Id {  get; set; }
        public string Value { get; set; } = default!;
        public int Advertisementid { get; set; }
        public int AttributeId { get; set; }
        public Advertisement Advertisement { get; set; } = default!;
        public Attribute Attribute { get; set;} = default!;
    }
}
