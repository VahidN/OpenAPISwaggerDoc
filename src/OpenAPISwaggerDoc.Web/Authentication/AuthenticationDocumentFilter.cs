using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenAPISwaggerDoc.Web.Authentication;

/// <summary>
///     Don't show the API's list to anonymous users
/// </summary>
public class AuthenticationDocumentFilter : IDocumentFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationDocumentFilter(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc == null)
        {
            throw new ArgumentNullException(nameof(swaggerDoc));
        }

        if (_httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == false)
        {
            swaggerDoc.Paths = new OpenApiPaths();
            swaggerDoc.Components.Schemas = new Dictionary<string, OpenApiSchema>();
        }
    }
}