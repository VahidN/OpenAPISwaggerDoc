using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using OpenAPISwaggerDoc.Profiles;
using OpenAPISwaggerDoc.Services;
using OpenAPISwaggerDoc.Web.AppConventions;
using OpenAPISwaggerDoc.Web.Authentication;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration);
var webApp = builder.Build();
ConfigureMiddlewares(webApp, webApp.Environment);
ConfigureEndpoints(webApp);
ConfigureDatabase(webApp);
webApp.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddCustomServices(configuration);
    services.AddHttpContextAccessor();
    services.AddAuthentication("Basic").AddBasicAuthenticationHandler();

    services.AddControllers(setupAction =>
                            {
                                setupAction.Filters.Add(new AuthorizeFilter());
                                setupAction.AddSwaggerMvcOptions();
                            })
            .AddJsonPatchSupport()
            .AddXmlDataContractSerializerFormatters();

    services.AddCustomApiBehaviorOptions();
    services.AddCustomAutoMapper();
    services.AddCustomSwaggerGen();
}

void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseSwagger();
    app.UseCustomSwaggerUI();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
}

void ConfigureEndpoints(IApplicationBuilder app)
{
    app.UseEndpoints(endpoints =>
                     {
                         endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                     });
}

void ConfigureDatabase(IApplicationBuilder app)
{
    app.ApplicationServices.ConfigureDatabase();
}