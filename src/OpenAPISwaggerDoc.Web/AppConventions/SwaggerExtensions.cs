using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace OpenAPISwaggerDoc.Web.AppConventions;

public static class SwaggerExtensions
{
    public static void AddSwaggerMvcOptions(this MvcOptions setupAction)
    {
        if (setupAction == null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

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

        // configure the NewtonsoftJsonOutputFormatter
        var jsonOutputFormatter = setupAction.OutputFormatters
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
    }

    public static void AddCustomSwaggerGen(this IServiceCollection services)
    {
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

    public static void UseCustomSwaggerUI(this IApplicationBuilder app)
    {
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
    }

    public static void AddCustomApiBehaviorOptions(this IServiceCollection services)
    {
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
    }

    public static IMvcBuilder AddJsonPatchSupport(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddNewtonsoftJson(setupAction =>
                                     {
                                         setupAction.SerializerSettings.ContractResolver =
                                             new CamelCasePropertyNamesContractResolver();
                                     });
        return mvcBuilder;
    }
}