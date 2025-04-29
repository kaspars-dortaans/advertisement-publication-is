namespace AdvertisementWebsite.Server.Dto.Message;

public class MessageAttachmentItemDto
{
    public string Url { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public long SizeInBytes { get; set; }
}
