using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SafeDose.Shared.Errors;

namespace SafeDose.Api.Auth;

// Used on internal endpoints (Langflow callbacks).
// NOT for patient routes - those use JWT.
// Reads X-Service-Token header and matches it to appsettings:Langflow:ServiceToken.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public class ServiceTokenAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var config = context.HttpContext.RequestServices
            .GetRequiredService<IConfiguration>();

        var expected = config["Langflow:ServiceToken"];
        if (string.IsNullOrEmpty(expected))
        {
            // Not configured = block. Fail closed.
            context.Result = new ObjectResult(new ErrorResponse(
                Code: ErrorCodes.Unauthorized,
                MessageArabic: ArabicMessages.Unauthorized))
            { StatusCode = 401 };
            await Task.CompletedTask;
            return;
        }

        var provided = context.HttpContext.Request.Headers["X-Service-Token"].ToString();
        if (provided != expected)
        {
            context.Result = new ObjectResult(new ErrorResponse(
                Code: ErrorCodes.Unauthorized,
                MessageArabic: ArabicMessages.Unauthorized))
            { StatusCode = 401 };
        }

        await Task.CompletedTask;
    }
}
