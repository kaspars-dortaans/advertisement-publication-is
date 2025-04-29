namespace BusinessLogic.Entities.Files;

public class MessageAttachment : File
{
    public int MessageId { get; set; }
    public long SizeInBytes { get; set; }
    public string FileName { get; set; } = default!;
    public Message Message { get; set; } = default!;
}
