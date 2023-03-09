using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

namespace OpenAPISwaggerDoc.Web.Authentication;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private string _failReason;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            _failReason = "Missing Authorization header";
            return Task.FromResult(AuthenticateResult.Fail(_failReason));
        }

        try
        {
            var authenticationHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authenticationHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var username = credentials[0];
            var password = credentials[1];

            if (string.Equals(username, "DNT", StringComparison.Ordinal) &&
                string.Equals(password, "123", StringComparison.Ordinal))
            {
                var claims = new[] { new Claim(ClaimTypes.NameIdentifier, username) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            _failReason = "Invalid username or password";
            return Task.FromResult(AuthenticateResult.Fail(_failReason));
        }
        catch (Exception ex)
        {
            _failReason = $"Invalid Authorization header: {ex.Message}";
            return Task.FromResult(AuthenticateResult.Fail(_failReason));
        }
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        await base.HandleChallengeAsync(properties);
        if (Response.StatusCode == StatusCodes.Status401Unauthorized &&
            !string.IsNullOrWhiteSpace(_failReason))
        {
            Response.Headers.Add("WWW-Authenticate", _failReason);
            Response.ContentType = "application/json";
            await WriteProblemDetailsAsync(_failReason);
        }
    }

    private Task WriteProblemDetailsAsync(string detail)
    {
        var problemDetails = new ProblemDetails { Detail = detail, Status = Context.Response.StatusCode };
        var result = new ObjectResult(problemDetails)
                     {
                         ContentTypes = new MediaTypeCollection(),
                         StatusCode = problemDetails.Status,
                         DeclaredType = problemDetails.GetType(),
                     };
        var executor = Context.RequestServices.GetRequiredService<IActionResultExecutor<ObjectResult>>();
        var routeData = Context.GetRouteData() ?? new RouteData();
        var actionContext = new ActionContext(Context, routeData, new ActionDescriptor());
        return executor.ExecuteAsync(actionContext, result);
    }
}