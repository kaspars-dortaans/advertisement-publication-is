using api.Authorization;
using api.Dto;
using api.Dto.Common;
using api.Dto.DataTableQuery;
using api.Dto.User;
using api.Entities;
using api.Helpers;
using api.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace api.Controllers;


[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly IBaseService<User> _userService;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    public UserController(IBaseService<User> userService, IMapper mapper, UserManager<User> userManager)
    {
        _userService = userService;
        _mapper = mapper;
        _userManager = userManager;
    }

    [HasPermission(Authorization.Permission.ViewUsers)]
    [HttpPost]
    public DataTableQueryResponse<UserListItem> GetUserList(DataTableQuery query)
    {
        var users = _userService.GetAll();
        var queryResult = users.ResolveDataTableQuery(query, null);
        var listItems = _mapper.MapDataTableResult<User, UserListItem>(queryResult);

        return listItems;
    }

    [AllowAnonymous]
    [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestError), StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        var createResult = await _userManager.CreateAsync(new User
        {
            Email = registerDto.Email,
            IsEmailPublic = registerDto.IsEmailPublic,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            UserName = registerDto.UserName,
            PhoneNumber = registerDto.PhoneNumber,
            IsPhoneNumberPublic = registerDto.IsPhoneNumberPublic
        }, registerDto.Password);

        //Handle user creation errors
        if (!createResult.Succeeded)
        {
            var errorCodes = new List<string>();
            foreach (var error in createResult.Errors)
            {
                if (error.Code.StartsWith(nameof(RegisterDto.Password)))
                {
                    ModelState.AddModelError(nameof(RegisterDto.Password), error.Code);
                } else if (error.Code.EndsWith(nameof(RegisterDto.Email)))
                {
                    ModelState.AddModelError(nameof(RegisterDto.Email), error.Code);
                } else if (error.Code.EndsWith(nameof(RegisterDto.UserName)))
                {
                    ModelState.AddModelError(nameof(RegisterDto.UserName), error.Code);
                } else
                {
                    errorCodes.Add(error.Code);
                }
            }
            var errors = ModelState
                .Where(e => e.Value != null)
                .ToDictionary(property => property.Key, property => property.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

            return new BadRequestObjectResult(new RequestError(errorCodes, errors));
        }

        return new OkResult();
    }
}
