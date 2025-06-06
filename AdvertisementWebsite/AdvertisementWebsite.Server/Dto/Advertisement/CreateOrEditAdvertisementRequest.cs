﻿using BusinessLogic.Constants;
using BusinessLogic.Dto.Image;
using BusinessLogic.Dto.Time;
using System.ComponentModel.DataAnnotations;
using AdvertisementWebsite.Server.Validators;

namespace AdvertisementWebsite.Server.Dto.Advertisement;

public class CreateOrEditAdvertisementRequest
{
    public int? Id { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerUserName { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public int CategoryId { get; set; }

    public IEnumerable<KeyValuePair<int, string>>? AttributeValues { get; set; }

    public PostTimeDto? PostTime { get; set; }

    /// <summary>
    /// Displayed on edit, readonly
    /// </summary>
    public DateTime? ValidToDate { get; set; }

    [MinLength(InputConstants.MinTitleLength, ErrorMessage = CustomErrorCodes.MinLength)]
    [MaxLength(InputConstants.MaxTitleLength, ErrorMessage = CustomErrorCodes.MaxLength)]
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Title { get; set; } = default!;

    [MaxLength(InputConstants.MaxTextLength, ErrorMessage = CustomErrorCodes.MaxLength)]
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Description { get; set; } = default!;

    [MaxFileSize(ImageConstants.MaxFileSizeInBytes)]
    [AllowedFileTypes(ImageConstants.AllowedFileTypes)]
    [MaxLength(ImageConstants.MaxImageCountPerAdvertisement, ErrorMessage = CustomErrorCodes.InvalidFileLimit)]
    public IEnumerable<IFormFile>? ImagesToAdd { get; set; }

    /// <summary>
    /// Image DTO's order representing image order.
    /// </summary>
    public IEnumerable<ImageDto>? ImageOrder { get; set; }
}
