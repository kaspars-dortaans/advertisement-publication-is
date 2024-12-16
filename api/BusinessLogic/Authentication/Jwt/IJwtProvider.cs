namespace BusinessLogic.Authentication.Jwt;

public interface IJwtProvider
{
    Task<string> GetJwtToken(string email, string password);
}
