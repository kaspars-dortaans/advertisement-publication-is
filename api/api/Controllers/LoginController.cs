using api.Authentication;
using api.Authentication.Jwt;
using api.Constants;
using api.Dto;
using api.Dto.Login;
using api.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class LoginController
{
    private readonly IJwtProvider _jwtProvider;

    public LoginController(IJwtProvider jwtProvider)
    {
        _jwtProvider = jwtProvider;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Authenticate(LoginDto request)
    {
        try
        {
            var token = await _jwtProvider.GetJwtToken(request.Email, request.Password);
            return new RequestResponse(token).ToObjectResult();
        } catch (Exception e)
        {
            var errorCode = e is InvalidCredentialException ? CustomErrorCodes.InvalidLoginCredentials : e.Message;
            return new RequestResponse(new List<string> { errorCode }, new Dictionary<string, IList<string>>()).ToObjectResult();
        }
    }
}
