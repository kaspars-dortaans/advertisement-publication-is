using BusinessLogic.Entities;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services;

public interface IUserService : IBaseService<User>
{
    public Task Register(User user, string password, IFormFile? profilePicture, IEnumerable<string>? roles);
    public IQueryable<Permission> GetUserPermissions(int userId);
    public Task UpdateUserInfo(User user, bool updateProfileImage, IFormFile? profileImage);
    public Task ChangePassword(int userId, string currentPassword, string newPassword);
}
