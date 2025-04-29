namespace BusinessLogic.Helpers.EmailClient;

public class EmailOptions
{
    public string SenderName { get; set; } = default!;
    public string SenderEmail { get; set; } = default!;
    public string AuthenticationPassword { get; set; } = default!;
    public string HostUrl { get; set; } = default!;
    public int HostPort { get; set; }

}
