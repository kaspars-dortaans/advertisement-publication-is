using BusinessLogic.Dto;
using BusinessLogic.Entities;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services;

public interface IUserService
{
    //TODO: fix this
    public Task<RequestResponse> Register(User user, string password, IFormFile? profilePicture);
}
