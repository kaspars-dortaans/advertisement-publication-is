using api.Dto.Login;
using api.Provaiders.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class LoginController
{
    private readonly IJwtProvider _jwtProvider;

    public LoginController(IJwtProvider jwtProvider) {
        _jwtProvider = jwtProvider;
    }

    [HttpPost]
    public async Task<IActionResult> Authenticate(LoginDto request)
    {
        try
        {
            var token = await _jwtProvider.GetJwtToken(request.Email, request.Password);
            return new OkObjectResult(token);
        } catch (Exception)
        {
            return new BadRequestResult();
        }
    }
}
