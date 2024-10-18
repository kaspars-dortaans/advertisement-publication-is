using api.Dto.User;
using api.Dto;

namespace api.Services;

public interface IUserService
{
    public Task<RequestResponse> Register(RegisterDto registerDto);
}
