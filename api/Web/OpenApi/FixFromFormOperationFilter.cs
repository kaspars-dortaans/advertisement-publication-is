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

        //If schema does not exist create it, it is automatically added to SchemaRepository
        if (!context.SchemaRepository.Schemas.ContainsKey(paramInfo.ParameterType.Name))
        {
            context.SchemaGenerator.GenerateSchema(paramInfo.ParameterType, context.SchemaRepository);
        }

        var dtoSchema = context.SchemaRepository.Schemas.First(s => s.Key == paramInfo.ParameterType.Name).Value;

        //Assign dtoSchema to mediType, it wont be referenced to object schema, but it will be included
        //TODO: Fix - assign schema with reference
        var mediaType = operation.RequestBody.Content.First().Value;
        mediaType.Schema = dtoSchema;

        mediaType.Encoding.Clear();
    }
}

