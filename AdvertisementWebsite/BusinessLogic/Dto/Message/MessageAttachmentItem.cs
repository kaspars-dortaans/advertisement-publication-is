namespace BusinessLogic.Dto.Message;

public class MessageAttachmentItem
{
    public int Id { get; set; }
    public string FileName { get; set; } = default!;
    public long SizeInBytes { get; set; }
}
