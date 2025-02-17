using BusinessLogic.Helpers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Web.OpenApi;

public class IncludeDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        //Load types which have IncludeInOpenApiSchema attribute from assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var includeTypes = assemblies.SelectMany(assembly => ReflectionHelper.GetTypesWithAttribute(assembly, typeof(IncludeInOpenApi)));

        //Add types to schema
        foreach(var type in includeTypes)
        {
            if (!context.SchemaRepository.Schemas.ContainsKey(type.Name))
            {
                context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository);
            }
        }
    }
}
