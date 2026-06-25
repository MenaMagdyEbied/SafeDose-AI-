using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SafeDose.Application.Interfaces;
using SafeDose.Shared.Errors;

namespace SafeDose.Api.Filters;

// Apply to endpoints that require an active paid subscription.
// Returns 402 Payment Required with an Arabic message if the caller is on the free tier.
public class RequiresPremiumAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var accountId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.HttpContext.User.FindFirstValue("nameid");

        if (string.IsNullOrEmpty(accountId))
        {
            context.Result = new UnauthorizedObjectResult(new ErrorResponse(
                ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));
            return;
        }

        var subs = context.HttpContext.RequestServices.GetRequiredService<ISubscriptionRepository>();
        var active = await subs.GetActiveByAccountAsync(accountId);
        if (active == null)
        {
            context.Result = new ObjectResult(new ErrorResponse(
                "PREMIUM_REQUIRED",
                "هذه الميزة تتطلب اشتراك بريميوم. يرجى الاشتراك للمتابعة."))
            {
                StatusCode = 402   // Payment Required
            };
            return;
        }

        await next();
    }
}
