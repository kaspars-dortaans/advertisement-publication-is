using BusinessLogic.Entities;
using BusinessLogic.Helpers.FilePathResolver;
using BusinessLogic.Helpers.Storage;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace Web.Controllers;

[ApiController]
[Authorize]
[Route("/[controller]/[action]")]
public class FileController(
    IBaseService<BusinessLogic.Entities.File> fileService,
    IStorage storage,
    IFilePathResolver filePathResolver) : ControllerBase
{
    private readonly IBaseService<BusinessLogic.Entities.File> _fileService = fileService;
    private readonly IStorage _storage = storage;
    private readonly IFilePathResolver _filePathResolver = filePathResolver;

    [AllowAnonymous]
    [ProducesResponseType<FileResult>((int)HttpStatusCode.OK)]
    [ProducesResponseType<NotFoundResult>((int)HttpStatusCode.NotFound)]
    [ProducesResponseType<ForbidResult>((int)HttpStatusCode.Forbidden)]
    [HttpGet(Name = nameof(GetFile))]
    public async Task<IActionResult> GetFile(int id)
    {
        var file = await _fileService.FirstOrDefaultAsync(f => f.Id == id);
        if (file is null)
        {
            return new NotFoundResult();
        }

        _ = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value, out int userId);

        //Perform file permission checks
        if (!file.IsPublic)
        {
            switch (file)
            {
                case UserFile:
                    var isFileOwner = file is UserFile
                            && (User?.Identity?.IsAuthenticated ?? false)
                            && userId == (file as UserFile)!.OwnerUserId;

                    if (!isFileOwner)
                    {
                        return Forbid();
                    }
                    break;
                default:
                    return Forbid();
            }
        }

        var fileData = await _storage.GetFile(file.Path);
        return File(fileData, "application/octet-stream", _filePathResolver.GetOriginalFileName(file.Path));
    }
}
