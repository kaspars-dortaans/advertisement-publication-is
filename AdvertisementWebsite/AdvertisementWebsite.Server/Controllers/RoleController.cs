using AdvertisementWebsite.Server.Dto.Roles;
using BusinessLogic.Authorization;
using BusinessLogic.Constants;
using BusinessLogic.Dto;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Roles;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AdvertisementWebsite.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public partial class RoleController(
    IRoleService roleService,
    IBaseService<Permission> permissionService,
    ILookupNormalizer lookupNormalizer
    ) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;
    private readonly IBaseService<Permission> _permissionService = permissionService;
    private readonly ILookupNormalizer _lookupNormalizer = lookupNormalizer;

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex SplitPascalCase();

    [HasPermission(Permissions.ViewAllRoles)]
    [ProducesResponseType<DataTableQueryResponse<RoleListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<DataTableQueryResponse<RoleListItem>> GetRoles(DataTableQuery request)
    {
        var result = await _roleService.GetRoles(request);
        return result;
    }

    [HasPermission(Permissions.ViewAllRoles)]
    [ProducesResponseType<IEnumerable<KeyValuePair<int, string>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IEnumerable<KeyValuePair<int, string>>> GetPermissionOptions()
    {
        var optionList = await _permissionService.GetAll().Select(p => new { p.Id, p.Name }).ToListAsync();
        return optionList.Select(o => new KeyValuePair<int, string>(o.Id, SplitPascalCase().Replace(o.Name, "$1 $2")));
    }

    [HasPermission(Permissions.AddRole)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task CreateRole(RoleFormRequest request)
    {
        var newRole = new Role()
        {
            Name = request.Name,
            NormalizedName = _lookupNormalizer.NormalizeName(request.Name),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            RolePermissions = request.Permissions.Select(p => new RolePermission
            {
                PermissionId = p,
            }).ToList(),
        };
        await _roleService.AddAsync(newRole);
    }

    [HasPermission(Permissions.ViewAllRoles)]
    [ProducesResponseType<RoleFormRequest>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<RoleFormRequest> GetRoleFormInfo(int id)
    {
        var roleInfo = (await _roleService
            .GetAll()
            .Select(r => new RoleFormRequest
            {
                Id = r.Id,
                Name = r.Name!,
                Permissions = r.Permissions.Select(p => p.Id)
            })
            .FirstOrDefaultAsync(r => r.Id == id))
            ?? throw new ApiException([CustomErrorCodes.NotFound]);
        return roleInfo;
    }

    [HasPermission(Permissions.EditRole)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task EditRole(RoleFormRequest request)
    {
        if (!request.Id.HasValue)
        {
            throw new ApiException([], new Dictionary<string, IList<string>>
            {
                { nameof(RoleFormRequest.Id), [CustomErrorCodes.MissingRequired] }
            });
        }

        var newRole = new Role()
        {
            Id = request.Id.Value,
            NormalizedName = _lookupNormalizer.NormalizeName(request.Name),
            Name = request.Name,
            RolePermissions = request.Permissions.Select(p => new RolePermission
            {
                PermissionId = p,
            }).ToList(),
        };
        await _roleService.UpdateRole(newRole);
    }

    [HasPermission(Permissions.DeleteRole)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task DeleteRoles(IEnumerable<int> ids)
    {
        await _roleService.Where(r => ids.Contains(r.Id))
            .ExecuteDeleteAsync();
    }
}
