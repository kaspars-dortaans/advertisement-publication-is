using AutoMapper;
using BusinessLogic.Authentication;
using BusinessLogic.Authentication.Jwt;
using BusinessLogic.Authorization;
using BusinessLogic.Constants;
using BusinessLogic.Dto;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Dto.Login;
using Web.Dto.User;
using Web.Helpers;

namespace Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class UserController(
    IUserService userService,
    UserManager<User> userManager,
    IMapper mapper,
    IJwtProvider jwtProvider) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly UserManager<User> _userManager = userManager;
    private readonly IMapper _mapper = mapper;
    private readonly IJwtProvider _jwtProvider = jwtProvider;

    [AllowAnonymous]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<string> Authenticate(LoginDto request)
    {
        try
        {
            var token = await _jwtProvider.GetJwtToken(request.Email, request.Password);
            return token;
        }
        catch (InvalidCredentialException)
        {
            throw new ApiException([CustomErrorCodes.InvalidLoginCredentials]);
        }
    }

    [HasPermission(Permissions.ViewUsers)]
    [HttpPost]
    public async Task<DataTableQueryResponse<UserListItem>> GetUserList(DataTableQuery query)
    {
        var users = _userService.GetAll();
        var queryResult = await users.ResolveDataTableQuery(query, null);
        var listItems = _mapper.MapDataTableResult<User, UserListItem>(queryResult);

        return listItems;
    }

    [AllowAnonymous]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
    {
        var user = _mapper.Map(registerDto, new User());
        await _userService.Register(user, registerDto.Password, registerDto.ProfileImage, [nameof(Roles.User)]);
        return Ok();
    }

    [AllowAnonymous]
    [ProducesResponseType<PublicUserInfoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
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

    [HasPermission(Permissions.ViewProfileInfo)]
    [HttpGet]
    public async Task<UserInfo> GetUserInfo()
    {
        var userId = User.GetUserId()!;
        var user = await _userManager
            .FindByIdAsync("" + userId.Value)
            ?? throw new ApiException([CustomErrorCodes.UserNotFound]);

        return _mapper.Map<UserInfo>(user, opt => opt.Items[nameof(Url)] = Url);
    }

    [HasPermission(Permissions.EditProfileInfo)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task UpdateUserInfo([FromForm] EditUserInfo request)
    {
        var userId = User.GetUserId()!.Value;

        var user = await _userService
            .Include(u => u.ProfileImageFile)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new ApiException([CustomErrorCodes.UserNotFound]);

        _mapper.Map(request, user);
        await _userService.UpdateUserInfo(user, request.ProfileImageChanged, request.ProfileImage);
    }

    [HttpGet]
    public async Task<IEnumerable<string>> GetCurrentUserPermissions()
    {
        var userId = User.GetUserId()!;
        var permissionNames = await _userService.GetUserPermissions(userId.Value)
            .Select(p => p.Name)
            .ToListAsync();

        return permissionNames;
    }

    [HasPermission(Permissions.ChangePassword)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task ChangePassword(ChangePasswordRequest request)
    {
        await _userService.ChangePassword(User.GetUserId()!.Value, request.CurrentPassword, request.Password);
    }
}
