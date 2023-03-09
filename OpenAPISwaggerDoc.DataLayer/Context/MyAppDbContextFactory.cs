using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OpenAPISwaggerDoc.DataLayer.Context;

public class MyAppDbContextFactory : IDesignTimeDbContextFactory<LibraryContext>
{
    public LibraryContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        WriteLine($"Using `{basePath}` as the BasePath");
        var configuration = new ConfigurationBuilder()
                            .SetBasePath(basePath)
                            .AddJsonFile("appsettings.json")
                            .Build();
        var builder = new DbContextOptionsBuilder<LibraryContext>();
        var connectionString = configuration.GetConnectionString("SqlServerConnection")
                                            .Replace("|DataDirectory|",
                                                     Path.Combine(basePath, "wwwroot", "app_data"),
                                                     StringComparison.OrdinalIgnoreCase);
        builder.UseSqlServer(connectionString);
        return new LibraryContext(builder.Options);
    }
}