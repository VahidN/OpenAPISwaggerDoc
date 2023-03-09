using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAPISwaggerDoc.DataLayer.Context;

namespace OpenAPISwaggerDoc.Services;

public static class ServicesExtensions
{
    public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, LibraryContext>();
        services.AddScoped<IBooksService, BooksService>();
        services.AddScoped<IAuthorsService, AuthorsService>();

        var connectionString = configuration.GetConnectionString("SqlServerConnection")
                                            .Replace("|DataDirectory|",
                                                     Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                                                                  "app_data"),
                                                     StringComparison.InvariantCultureIgnoreCase);
        services.AddDbContext<LibraryContext>(options =>
                                              {
                                                  options.UseSqlServer(connectionString,
                                                                       dbOptions =>
                                                                       {
                                                                           var minutes = (int)TimeSpan.FromMinutes(3)
                                                                               .TotalSeconds;
                                                                           dbOptions.CommandTimeout(minutes);
                                                                           dbOptions.EnableRetryOnFailure();
                                                                       })
                                                         .EnableSensitiveDataLogging();
                                              });
    }

    public static void ConfigureDatabase(this IServiceProvider applicationServices)
    {
        using var scope = applicationServices.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        uow.Migrate();
    }
}