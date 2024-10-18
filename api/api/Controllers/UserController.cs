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
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

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

    [HasPermission(Authorization.Permission.ViewUsers)]
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
        return (await _userService.Register(registerDto)).ToObjectResult();
    }
}
