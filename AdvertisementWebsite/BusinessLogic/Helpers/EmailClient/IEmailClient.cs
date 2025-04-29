using BusinessLogic.Dto.Email;

namespace BusinessLogic.Helpers.EmailClient;

public interface IEmailClient
{
    public Task SendEmail(SendEmailDto dto);
}
