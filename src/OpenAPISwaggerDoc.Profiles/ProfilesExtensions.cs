using Microsoft.Extensions.DependencyInjection;

namespace OpenAPISwaggerDoc.Profiles;

public static class ProfilesExtensions
{
    public static void AddCustomAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(BookProfile).GetTypeInfo().Assembly);
    }
}