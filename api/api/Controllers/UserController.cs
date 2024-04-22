using api.Dto.Common;
using api.Dto.DataTableQuery;
using api.Dto.User;
using api.Entities;
using api.Helpers;
using api.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;


namespace api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IBaseService<User> _userService;
        private readonly IMapper _mapper;
        public UserController(IBaseService<User> userService, IMapper mapper) {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost]
        public DataTableQueryResponse<UserListItem> GetUserList(DataTableQuery query) {
            var users = _userService.GetAll();
            var queryResult = users.ResolveDataTableQuery(query, null);
            var listItems = _mapper.MapDataTableResult<User, UserListItem>(queryResult);

            return listItems;
        }
    }
}
