using api.Authentication;
using api.Authentication.Jwt;
using api.Constants;
using api.Dto;
using api.Dto.Login;
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
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Authenticate(LoginDto request)
    {
        try
        {
            var token = await _jwtProvider.GetJwtToken(request.Email, request.Password);
            return new OkObjectResult(token);
        } catch (InvalidCredentialException)
        {
            var error = new RequestError(CustomErrorCodes.InvalidLoginCredentials);
            return new BadRequestObjectResult(error);
        } catch (Exception)
        {
            return new BadRequestResult();
        }
    }
}
