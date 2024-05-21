namespace api.Provaiders.Jwt;

public interface IJwtProvider
{
    Task<string> GetJwtToken(string email, string password);
}
