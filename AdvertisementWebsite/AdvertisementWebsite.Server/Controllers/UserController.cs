using AutoMapper;
using BusinessLogic.Authorization;
using BusinessLogic.Constants;
using BusinessLogic.Dto;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.CookieSettings;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using System.Security.Claims;
using AdvertisementWebsite.Server.Dto.Login;
using AdvertisementWebsite.Server.Dto.User;
using AdvertisementWebsite.Server.Helpers;

namespace AdvertisementWebsite.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class UserController(
    IUserService userService,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IMapper mapper,
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
    CookieSettingsHelper cookieSettingHelper) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly UserManager<User> _userManager = userManager;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly IMapper _mapper = mapper;
    private readonly CookieSettingsHelper _cookieSettingHelper = cookieSettingHelper;
    private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions = bearerTokenOptions;

    [AllowAnonymous]
    [ProducesResponseType<AccessTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult>> Login(LoginDto request)
    {
        try
        {
            _signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
            var user = (await _userManager.FindByEmailAsync(request.Email)) ?? throw new InvalidCredentialException();
            var signInAttempt = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (signInAttempt != Microsoft.AspNetCore.Identity.SignInResult.Success)
            {
                throw new InvalidCredentialException();
            }

            //Set last user language
            var claims = await _userManager.GetClaimsAsync(user!);
            var localeClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Locality);
            if (localeClaim != null && localeClaim.Value != _cookieSettingHelper.Settings.Locale)
            {
                _cookieSettingHelper.Settings.Locale = localeClaim.Value;
                _cookieSettingHelper.AttachToResponse();
            }

            await _signInManager.SignInAsync(user, false);

            //Access token is attached to response by bearer token sign in handler, which is called within PasswordSingInAsync
            return TypedResults.Empty;
        }
        catch (InvalidCredentialException)
        {
            throw new ApiException([CustomErrorCodes.InvalidLoginCredentials]);
        }
    }

    [ProducesResponseType<Ok>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task Logout()
    {
        _signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
        await _signInManager.SignOutAsync();
    }

    [AllowAnonymous]
    [ProducesResponseType<AccessTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<UnauthorizedHttpResult>(StatusCodes.Status401Unauthorized)]
    [HttpPost]
    public async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>> Refresh(RefreshRequest refreshRequest)
    {
        var refreshTokenProtector = _bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

        // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
        if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc 
            || DateTime.UtcNow >= expiresUtc 
            || await _signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not User user
        )
        {
            return TypedResults.Challenge();
        }

        var newPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
        return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
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
        var user = await _userService.Include(u => u.ProfileImageFile)
            .FirstOrDefaultAsync(u => u.Id == userId)
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

    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task SetLanguage([MaxLength(4)] string language)
    {
        var user = await _userManager.FindByIdAsync("" + User.GetUserId()!.Value) ?? throw new ApiException([CustomErrorCodes.UserNotFound]);
        var userClaims = await _userManager.GetClaimsAsync(user);
        var existingLanguageClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Locality);
        var newClaim = new Claim(ClaimTypes.Locality, language);
        if (existingLanguageClaim is not null)
        {
            await _userManager.ReplaceClaimAsync(user!, existingLanguageClaim, newClaim);
        }
        else
        {
            await _userManager.AddClaimAsync(user, newClaim);
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
}
