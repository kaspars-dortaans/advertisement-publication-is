namespace api.Provaiders.Jwt
{
    public class JwtProviderOptions
    {
        public string SecretKey { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public int TokenExpirationTimeInMinutes { get; set; }
    }
}
