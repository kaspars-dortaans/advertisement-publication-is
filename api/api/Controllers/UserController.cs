using api.Dto.User;
using api.Entities;
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

        [HttpGet]
        public IEnumerable<UserListItem> GetUserList() {
            var users = _userService.GetAll();
            var listItems = _mapper.Map<IEnumerable<UserListItem>>(users);
            
            return listItems;
        }
    }
}
