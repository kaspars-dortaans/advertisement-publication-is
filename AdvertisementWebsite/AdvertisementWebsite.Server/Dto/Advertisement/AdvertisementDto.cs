﻿using BusinessLogic.Dto.Advertisement;
using BusinessLogic.Dto.Image;

namespace AdvertisementWebsite.Server.Dto.Advertisement;

public class AdvertisementDto
{
    public int CategoryId { get; set; }
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string AdvertisementText { get; set; } = default!;
    public DateTime PostedDate { get; set; }
    public int ViewCount { get; set; }
    public bool? IsBookmarked { get; set; }
    public IEnumerable<AttributeValueItem> Attributes { get; set; } = [];
    public IEnumerable<ImageUrl> ImageURLs{ get; set; } = [];

    public int OwnerId { get; set; }
    public string? MaskedAdvertiserPhoneNumber { get; set; }
    public string? MaskedAdvertiserEmail { get; set; }
}
