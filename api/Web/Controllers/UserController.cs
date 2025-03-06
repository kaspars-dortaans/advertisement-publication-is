using AutoMapper;
using BusinessLogic.Authorization;
using BusinessLogic.Dto;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;
using BusinessLogic.Helpers;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Dto.User;

namespace Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class UserController(
    IUserService userService,
    UserManager<User> userManager,
    IMapper mapper) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly UserManager<User> _userManager = userManager;
    private readonly IMapper _mapper = mapper;

    [HasPermission(BusinessLogic.Authorization.Permission.ViewUsers)]
    [HttpPost]
    public async Task<DataTableQueryResponse<UserListItem>> GetUserList(DataTableQuery query)
    {
        var users = _userService.GetAll();
        var queryResult = await users.ResolveDataTableQuery(query, null);
        var listItems = _mapper.MapDataTableResult<User, UserListItem>(queryResult);

        return listItems;
    }

    [AllowAnonymous]
    [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestExceptionResponse), StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
    {
        var user = _mapper.Map(registerDto, new User());
        await _userService.Register(user, registerDto.Password, registerDto.ProfileImage, [nameof(Roles.User)]);
        return Ok();
    }

    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicUserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestExceptionResponse), StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<IActionResult> GetPublicUserInfo(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return NotFound();
        }

        var isInUserRole = await _userManager.IsInRoleAsync(user, nameof(Roles.User));
        //Check if user has regular user role, to not publicly expose admin accounts
        if (!isInUserRole)
        {
            return NotFound();
        }

        var res = _mapper.Map<PublicUserInfoDto>(user, o => o.Items[nameof(Url)] = Url);
        return Ok(res);
    }
}
