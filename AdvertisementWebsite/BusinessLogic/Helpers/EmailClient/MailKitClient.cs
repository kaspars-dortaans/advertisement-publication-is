using BusinessLogic.Dto.Email;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BusinessLogic.Helpers.EmailClient;

public class MailKitClient(IOptions<EmailOptions> emailOptions, ILogger<MailKitClient> logger) : IEmailClient
{
    private readonly IOptions<EmailOptions> _emailOptions = emailOptions;
    private readonly ILogger _logger = logger;

    public async Task SendEmail(SendEmailDto dto)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailOptions.Value.SenderName, _emailOptions.Value.SenderEmail));
            message.To.Add(new MailboxAddress(dto.ReceiverName, dto.ReceiverEmail));
            message.Subject = dto.Subject;
            message.Body = new TextPart(dto.IsBodyHtml ? "html" : "plain")
            {
                Text = dto.EmailBody
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(_emailOptions.Value.HostUrl, _emailOptions.Value.HostPort, SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_emailOptions.Value.SenderEmail, _emailOptions.Value.AuthenticationPassword);
            var response = await client.SendAsync(message);

            await client.DisconnectAsync(true);
        }
        catch (Exception e)
        {
            _logger.LogError("Error while sending email, message {message}\n stack trace: {stackTrace}", e.Message, e.StackTrace);
        }
    }
}
