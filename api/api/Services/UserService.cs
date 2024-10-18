using api.Constants;
using api.Dto;
using api.Dto.User;
using api.Entities;
using api.Helpers.FilePathResolver;
using api.Helpers.Storage;
using Microsoft.AspNetCore.Identity;
namespace api.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly IStorage _storage;
    private readonly IFilePathResolver _filePathResolver;
    private readonly IBaseService<Entities.File> _fileService;

    public UserService(
        UserManager<User> userManager,
        IStorage storage,
        IFilePathResolver filePathResolver,
        IBaseService<Entities.File> fileService)
    {
        _userManager = userManager;
        _storage = storage;
        _filePathResolver = filePathResolver;
        _fileService = fileService;
    }

    /// <summary>
    /// Register new user
    /// </summary>
    /// <param name="registerDto"></param>
    /// <returns></returns>
    public async Task<RequestResponse> Register(RegisterDto registerDto)
    {
        var createResult = await _userManager.CreateAsync(new User
        {
            Email = registerDto.Email,
            IsEmailPublic = registerDto.IsEmailPublic,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            UserName = registerDto.UserName,
            PhoneNumber = registerDto.PhoneNumber,
            IsPhoneNumberPublic = registerDto.IsPhoneNumberPublic
        }, registerDto.Password);

        //Handle user creation errors
        var response = HandleCreateUserResult(createResult);
        if (!response.Successful)
        {
            return response;
        }

        if (registerDto.ProfileImage is null)
        {
            return response;
        }

        //Save ProfileImage
        return await SaveProfileImage(registerDto.Email, registerDto.ProfileImage);
    }

    /// <summary>
    /// Handle created user result, return errors if any
    /// </summary>
    /// <param name="createResult"></param>
    /// <returns></returns>
    private static RequestResponse HandleCreateUserResult(IdentityResult createResult)
    {
        var response = new RequestResponse();
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                if (error.Code.StartsWith(nameof(RegisterDto.Password)))
                {
                    response.AddError(nameof(RegisterDto.Password), error.Code);
                } else if (error.Code.EndsWith(nameof(RegisterDto.Email)))
                {
                    response.AddError(nameof(RegisterDto.Email), error.Code);
                } else if (error.Code.EndsWith(nameof(RegisterDto.UserName)))
                {
                    response.AddError(nameof(RegisterDto.UserName), error.Code);
                } else
                {
                    response.AddErrorCode(error.Code);
                }
            }
        }
        return response;
    }

    /// <summary>
    /// Save profile image for user. Adds File entity, updates user profile image reference, saves profile image in storage.
    /// Does not delete previous profile image from storage or it's file entity.
    /// </summary>
    /// <param name="userEmail"></param>
    /// <param name="profileImage"></param>
    /// <returns></returns>
    public async Task<RequestResponse> SaveProfileImage(string userEmail, IFormFile profileImage)
    {
        var response = new RequestResponse();

        //Get created user
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is null)
        {
            response.AddErrorCode(CustomErrorCodes.UserNotFound);
            return response;
        }

        //Add file entity
        var filePath = _filePathResolver.GenerateUniqueFilePath(FileFolderConstants.ProfileImageFolder, profileImage.FileName);
        var file = await _fileService.AddAsync(new Entities.File()
        {
            OwnerUserId = user.Id,
            Path = filePath
        });

        //Update user profile image reference
        user.ProfileImageFileId = file.Id;
        await _userManager.UpdateAsync(user);

        //Save file data in storage
        var fileReadStream = profileImage.OpenReadStream();
        await _storage.PutFile(filePath, fileReadStream);

        return response;
    }
}
