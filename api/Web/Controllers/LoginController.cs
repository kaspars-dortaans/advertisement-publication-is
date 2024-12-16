using BusinessLogic.Authentication;
using BusinessLogic.Authentication.Jwt;
using BusinessLogic.Constants;
using BusinessLogic.Dto;
using BusinessLogic.Helpers;
using Microsoft.AspNetCore.Mvc;
using Web.Dto.Login;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class LoginController(IJwtProvider jwtProvider)
{
    private readonly IJwtProvider _jwtProvider = jwtProvider;

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
