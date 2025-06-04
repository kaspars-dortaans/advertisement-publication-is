using AdvertisementWebsite.Server.Dto.Permission;
using BusinessLogic.Authorization;
using BusinessLogic.Constants;
using BusinessLogic.Dto;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Permission;
using BusinessLogic.Exceptions;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementWebsite.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class PermissionController(
        IPermissionService permissionService) : ControllerBase
{

    private readonly IPermissionService _permissionService = permissionService;

    [HasPermission(Permissions.ViewAllPermissions)]
    [ProducesResponseType<DataTableQueryResponse<PermissionListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<DataTableQueryResponse<PermissionListItem>> GetPermissionList(DataTableQuery request)
    {
        return await _permissionService.GetPermissionList(request);
    }

    [HasPermission(Permissions.AddPermission)]
    [ProducesResponseType<Ok>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task CreatePermission(PutPermissionRequest request)
    {
        await _permissionService.AddAsync(new BusinessLogic.Entities.Permission()
        {
            Name = request.Name,
        });
    }

    [HasPermission(Permissions.EditPermission)]
    [ProducesResponseType<Ok>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task EditPermission(PutPermissionRequest request)
    {
        if(request.Id == null)
        {
            throw new ApiException([], new Dictionary<string, IList<string>>
            {
                { nameof(PutPermissionRequest.Id), [CustomErrorCodes.MissingRequired] }
            });
        }

        var permission = (await _permissionService.FirstOrDefaultAsync(p => p.Id == request.Id)) 
            ?? throw new ApiException([CustomErrorCodes.NotFound]);
        
        permission.Name = request.Name;
        await _permissionService.UpdateAsync(permission);
    }

    [HasPermission(Permissions.DeletePermission)]
    [ProducesResponseType<Ok>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task DeletePermissions(IEnumerable<int> ids)
    {
        await _permissionService.DeleteWhereAsync(p => ids.Contains(p.Id));
    }
}
