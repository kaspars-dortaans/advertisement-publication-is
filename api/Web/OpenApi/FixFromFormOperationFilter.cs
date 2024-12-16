using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Web.OpenApi;

/// <summary>
/// Add missing Dto used with FromForm attribute to openApi definition
/// Solution found: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2592#issuecomment-1397498558
/// </summary>
public class FixFromFormOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        //If http method is not post or put return
        if (context.ApiDescription.HttpMethod != "POST" && context.ApiDescription.HttpMethod != "PUT")
            return;

        //If endpoint does not have argument with FromForm attribute return
        var paramInfo = context.MethodInfo.GetParameters().FirstOrDefault(p => p.GetCustomAttribute<FromFormAttribute>() is not null);
        if (paramInfo is null)
            return;

        OpenApiSchema referenceSchema;
        //If schema does not exist create it, it is automatically added to SchemaRepository
        if (!context.SchemaRepository.Schemas.ContainsKey(paramInfo.ParameterType.Name))
        {
            referenceSchema = context.SchemaGenerator.GenerateSchema(paramInfo.ParameterType, context.SchemaRepository);
        }
    }
}

