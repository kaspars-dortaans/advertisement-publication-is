using BusinessLogic.Constants;
using BusinessLogic.Helpers;
using AdvertisementWebsite.Server.Validators;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Message;

[IncludeInOpenApi]
public class SendMessageRequest
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public int ChatId { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Text { get; set; } = default!;

    [MaxFileSize(AttachmentsConstants.MaxAttachmentSizeInBytes)]
    [MaxLength(AttachmentsConstants.MaxAttachmentCount, ErrorMessage = CustomErrorCodes.InvalidFileLimit)]
    public IEnumerable<IFormFile>? Attachments { get; set; }
}