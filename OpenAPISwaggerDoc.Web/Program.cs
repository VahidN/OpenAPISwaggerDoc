using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using OpenAPISwaggerDoc.DataLayer.Context;
using OpenAPISwaggerDoc.Profiles;
using OpenAPISwaggerDoc.Services;
using OpenAPISwaggerDoc.Web.Authentication;
using Swashbuckle.AspNetCore.SwaggerUI;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

var builder = WebApplication.CreateBuilder(args);
ConfigureLogging(builder.Logging, builder.Environment, builder.Configuration);
ConfigureServices(builder.Services, builder.Configuration);
var webApp = builder.Build();
ConfigureMiddlewares(webApp, webApp.Environment);
ConfigureEndpoints(webApp);
ConfigureDatabase(webApp);
webApp.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddScoped<IUnitOfWork, LibraryContext>();
    services.AddScoped<IBooksService, BooksService>();
    services.AddScoped<IAuthorsService, AuthorsService>();

    services.AddHttpContextAccessor();

    services.AddAuthentication("Basic")
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);

    var connectionString = configuration.GetConnectionString("SqlServerConnection")
                                        .Replace("|DataDirectory|",
                                                 Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                                                              "app_data"), StringComparison.InvariantCultureIgnoreCase);
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


    services.AddControllers(setupAction =>
                            {
                                setupAction.Filters.Add(new AuthorizeFilter());

                                setupAction.OutputFormatters.Add(new XmlSerializerOutputFormatter());

                                setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes
                                                            .Status400BadRequest));
                                setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes
                                                            .Status406NotAcceptable));
                                setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes
                                                            .Status500InternalServerError));
                                setupAction.Filters.Add(new ProducesDefaultResponseTypeAttribute());
                                setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes
                                                            .Status401Unauthorized));
                                setupAction.ReturnHttpNotAcceptable = true; // Status406NotAcceptable
                            }).AddNewtonsoftJson(setupAction =>
                                                 {
                                                     setupAction.SerializerSettings.ContractResolver =
                                                         new CamelCasePropertyNamesContractResolver();
                                                 }) // for json-patch
            .AddXmlDataContractSerializerFormatters();

// configure the NewtonsoftJsonOutputFormatter
    services.Configure<MvcOptions>(configureOptions =>
                                   {
                                       var jsonOutputFormatter = configureOptions.OutputFormatters
                                           .OfType<NewtonsoftJsonOutputFormatter>().FirstOrDefault();

                                       if (jsonOutputFormatter != null)
                                       {
                                           // remove text/json as it isn't the approved media type
                                           // for working with JSON at API level
                                           if (jsonOutputFormatter.SupportedMediaTypes.Contains("text/json"))
                                           {
                                               jsonOutputFormatter.SupportedMediaTypes.Remove("text/json");
                                           }
                                       }
                                   });

    services.Configure<ApiBehaviorOptions>(options =>
                                           {
                                               options.InvalidModelStateResponseFactory = actionContext =>
                                               {
                                                   var actionExecutingContext =
                                                       actionContext as ActionExecutingContext;

                                                   // if there are modelstate errors & all keys were correctly
                                                   // found/parsed we're dealing with validation errors
                                                   if (actionContext.ModelState.ErrorCount > 0
                                                       && actionExecutingContext?.ActionArguments.Count ==
                                                       actionContext.ActionDescriptor.Parameters.Count)
                                                   {
                                                       return new UnprocessableEntityObjectResult(actionContext
                                                           .ModelState);
                                                   }

                                                   // if one of the keys wasn't correctly found / couldn't be parsed
                                                   // we're dealing with null/unparsable input
                                                   return new BadRequestObjectResult(actionContext.ModelState);
                                               };
                                           });

    services.AddAutoMapper(typeof(BookProfile).GetTypeInfo().Assembly);

    services.AddSwaggerGen(setupAction =>
                           {
                               // setupAction.DocumentFilter<AuthenticationDocumentFilter>()
                               setupAction.SwaggerDoc(
                                                      "LibraryOpenAPISpecification",
                                                      new OpenApiInfo
                                                      {
                                                          Title = "Library API",
                                                          Version = "1",
                                                          Description =
                                                              "Through this API you can access authors and their books.",
                                                          Contact = new OpenApiContact
                                                                    {
                                                                        Email = "name@site.com",
                                                                        Name = "DNT",
                                                                        Url = new Uri("https://www.dntips.ir"),
                                                                    },
                                                          License = new OpenApiLicense
                                                                    {
                                                                        Name = "MIT License",
                                                                        Url =
                                                                            new
                                                                                Uri("https://opensource.org/licenses/MIT"),
                                                                    },
                                                      });

                               /*var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                               var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);
                               setupAction.IncludeXmlComments(xmlCommentsFullPath);*/

                               var xmlFiles = Directory
                                              .GetFiles(AppContext.BaseDirectory, "*.xml",
                                                        SearchOption.TopDirectoryOnly).ToList();
                               xmlFiles.ForEach(xmlFile => setupAction.IncludeXmlComments(xmlFile));

                               setupAction.AddSecurityDefinition("basicAuth", new OpenApiSecurityScheme
                                                                              {
                                                                                  Type = SecuritySchemeType.Http,
                                                                                  Scheme = "basic",
                                                                                  Description =
                                                                                      "Input your username and password to access this API",
                                                                              });

                               setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
                                                                  {
                                                                      {
                                                                          new OpenApiSecurityScheme
                                                                          {
                                                                              Reference = new OpenApiReference
                                                                                  {
                                                                                      Type = ReferenceType
                                                                                          .SecurityScheme,
                                                                                      Id = "basicAuth",
                                                                                  },
                                                                          },
                                                                          new List<string>()
                                                                      },
                                                                  });
                           });
}

void ConfigureLogging(ILoggingBuilder logging, IHostEnvironment env, IConfiguration configuration)
{
    logging.ClearProviders();

    if (env.IsDevelopment())
    {
        logging.AddDebug();
        logging.AddConsole();
    }

    logging.AddConfiguration(configuration.GetSection("Logging"));
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
    app.UseSwaggerUI(setupAction =>
                     {
                         /*setupAction.InjectStylesheet("/Assets/custom-ui.css");
                         setupAction.IndexStream = ()
                                 => GetType().Assembly
                                 .GetManifestResourceStream("OpenAPISwaggerDoc.Web.EmbeddedAssets.index.html");*/

                         setupAction.SwaggerEndpoint("/swagger/LibraryOpenAPISpecification/swagger.json",
                                                     "Library API");
                         setupAction.RoutePrefix = "";

                         setupAction.DefaultModelExpandDepth(2);
                         setupAction.DefaultModelRendering(ModelRendering.Model);
                         setupAction.DocExpansion(DocExpansion.None);
                         setupAction.EnableDeepLinking();
                         setupAction.DisplayOperationId();
                     });

    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();

    app.UseAuthorization();
}

void ConfigureEndpoints(IApplicationBuilder app)
{
    app.UseEndpoints(endpoints =>
                     {
                         endpoints.MapControllerRoute("default",
                                                      "{controller=Home}/{action=Index}/{id?}");
                     });
}

void ConfigureDatabase(IApplicationBuilder app)
{
    using var scope = app.ApplicationServices.CreateScope();
    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    uow.Migrate();
}