using Microsoft.AspNetCore.Authentication;

namespace OpenAPISwaggerDoc.Web.Authentication;

public static class BasicAuthenticationHandlerExtensions
{
    public static void AddBasicAuthenticationHandler(this AuthenticationBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);
    }
}