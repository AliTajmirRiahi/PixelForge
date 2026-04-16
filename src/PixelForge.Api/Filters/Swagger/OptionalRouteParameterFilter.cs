using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PixelForge.Api.Filters.Swagger
{
    public class OptionalRouteParameterFilter : IParameterFilter
    {
        public void Apply(IOpenApiParameter parameter, ParameterFilterContext context)
        {
            if (parameter.In == ParameterLocation.Path && parameter.Required && parameter is OpenApiParameter)
            {
                if (context.ParameterInfo.CustomAttributes.Any(p => p.NamedArguments.Any(p => p.MemberName == "Required" && ((bool?)p.TypedValue.Value == false))))
                    (parameter as OpenApiParameter)?.Required = false;
            }
        }
    }
}
