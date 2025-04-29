namespace BusinessLogic.Dto.Email;

public class SendEmailDto
{
    public string ReceiverEmail { get; set; } = default!;
    public string ReceiverName { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string EmailBody { get; set; } = default!;
    public bool IsBodyHtml { get; set; }
}
