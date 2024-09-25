using api.Authorization;
using api.Dto.Common;
using api.Dto.DataTableQuery;
using api.Dto.User;
using api.Entities;
using api.Helpers;
using api.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
namespace api.Controllers;


[Authorize]
[ApiController]
    [Route("api/[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly IBaseService<User> _userService;
    public UserController(IBaseService<User> userService, IMapper mapper) : base(mapper)
    {
        _userService = userService;
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
}
