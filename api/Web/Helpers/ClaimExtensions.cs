using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Web.Helpers;

public static class ClaimExtensions
{
    public static int? GetClaimValueInt(this ClaimsPrincipal userClaims, string claimName)
    {
        var claim = userClaims.Claims.FirstOrDefault(c => c.Type == claimName);
        return int.TryParse(claim?.Value, out int intValue) 
            ? intValue 
            : null;
    }

    public static int? GetUserId(this ClaimsPrincipal userClaims)
    {
        return GetClaimValueInt(userClaims, ClaimTypes.NameIdentifier);
    }
}
