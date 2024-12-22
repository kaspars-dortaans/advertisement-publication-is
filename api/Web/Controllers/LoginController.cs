using BusinessLogic.Authentication;
using BusinessLogic.Authentication.Jwt;
using BusinessLogic.Constants;
using BusinessLogic.Dto;
using BusinessLogic.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Web.Dto.Login;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class LoginController(IJwtProvider jwtProvider)
{
    private readonly IJwtProvider _jwtProvider = jwtProvider;

    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestExceptionResponse), StatusCodes.Status400BadRequest)]
    public async Task<string> Authenticate(LoginDto request)
    {
        try
        {
            var token = await _jwtProvider.GetJwtToken(request.Email, request.Password);
            return token;
        } catch (InvalidCredentialException)
        {
            throw new ApiException([CustomErrorCodes.InvalidLoginCredentials]);
        }
    }
}
