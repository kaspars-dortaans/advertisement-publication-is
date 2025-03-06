using BusinessLogic.Entities;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services;

public interface IUserService : IBaseService<User>
{
    public Task Register(User user, string password, IFormFile? profilePicture, IEnumerable<string>? roles);
}
