using AutoMapper;
using BusinessLogic.Authorization;
using BusinessLogic.Dto;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;
using BusinessLogic.Helpers;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Dto.User;

namespace Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly IBaseService<User> _userBaseService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public UserController(
        IBaseService<User> userBaseService,
        IUserService userService,
        IMapper mapper)
    {
        _userBaseService = userBaseService;
        _userService = userService;
        _mapper = mapper;
    }

    [HasPermission(BusinessLogic.Authorization.Permission.ViewUsers)]
    [HttpPost]
    public DataTableQueryResponse<UserListItem> GetUserList(DataTableQuery query)
    {
        var users = _userBaseService.GetAll();
        var queryResult = users.ResolveDataTableQuery(query, null);
        var listItems = _mapper.MapDataTableResult<User, UserListItem>(queryResult);

        return listItems;
    }

    [AllowAnonymous]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
    {
        var user = _mapper.Map(registerDto, new User());
        return (await _userService.Register(user, registerDto.Password, registerDto.ProfileImage)).ToObjectResult();
    }
}
