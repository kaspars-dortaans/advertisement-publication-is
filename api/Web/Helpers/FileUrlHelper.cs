using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;

namespace Web.Helpers;

public static class FileUrlHelper
{
    public static string? MapperGetFileUrl(ResolutionContext context, int? id)
    {
        if (context.Items[nameof(ControllerBase.Url)] is not IUrlHelper urlHelper)
        {
            return null;
        }
        return GetFileUrl(urlHelper, id);
    }

    public static string? GetFileUrl(this IUrlHelper urlHelper, int? id)
    {
        if (id == null)
        {
            return null;
        }
        return urlHelper.Link(nameof(FileController.GetFile), new { id });
    }

    public static string? MapperGetThumbnailUrl(ResolutionContext context, int? id)
    {
        if (context.Items[nameof(ControllerBase.Url)] is not IUrlHelper urlHelper)
        {
            return null;
        }
        return GetThumbnailUrl(urlHelper, id);
    }

    public static string? GetThumbnailUrl(this IUrlHelper urlHelper, int? id)
    {
        if (id is null)
        {
            return null;
        }
        return urlHelper.Link(nameof(FileController.GetFile), new { id, getThumbnail = true });
    }
}
