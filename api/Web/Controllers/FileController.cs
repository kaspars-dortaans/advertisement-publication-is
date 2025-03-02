using BusinessLogic.Entities.Files;
using BusinessLogic.Helpers.FilePathResolver;
using BusinessLogic.Helpers.Storage;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Web.Helpers;

namespace Web.Controllers;

[ApiController]
[Authorize]
[Route("/[controller]/[action]")]
public class FileController(
    IFileService fileService,
    IStorage storage,
    IFilePathResolver filePathResolver) : ControllerBase
{
    private readonly IFileService _fileService = fileService;
    private readonly IStorage _storage = storage;
    private readonly IFilePathResolver _filePathResolver = filePathResolver;

    [AllowAnonymous]
    [ProducesResponseType<FileResult>((int)HttpStatusCode.OK)]
    [ProducesResponseType<NotFoundResult>((int)HttpStatusCode.NotFound)]
    [ProducesResponseType<ForbidResult>((int)HttpStatusCode.Forbidden)]
    [HttpGet(Name = nameof(GetFile))]
    public async Task<IActionResult> GetFile(int id, bool getThumbnail)
    {
        var file = await _fileService.FirstOrDefaultAsync(f => f.Id == id);
        if (file is null)
        {
            return new NotFoundResult();
        }

        var userId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
        if (!_fileService.HasAccessToFile(file, userId))
        {
            return Forbid();
        }

        var filePath = "";
        if (getThumbnail)
        {
            if (file is not Image image)
            {
                return BadRequest();
            }
            filePath = image.ThumbnailPath;
        }
        else
        {
            filePath = file.Path;
        }

        var fileData = await _storage.GetFile(filePath);
        if (fileData is null)
        {
            return NotFound();
        }

        return File(fileData, "application/octet-stream", _filePathResolver.GetOriginalFileName(file.Path));
    }
}
