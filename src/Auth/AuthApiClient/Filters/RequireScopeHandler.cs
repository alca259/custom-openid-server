using Microsoft.AspNetCore.Authorization;
using OpenIddict.Abstractions;

namespace AuthApiClient.Filters;

public sealed class RequireScopeHandler : AuthorizationHandler<RequireScope>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, RequireScope requirement)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (requirement == null)
            throw new ArgumentNullException(nameof(requirement));

        var scopeClaim = context.User.Claims.FirstOrDefault(t => t.Type == "scope");


        if (scopeClaim != null)
        {
            bool valid = true;

            foreach (var scopeName in requirement.Scopes)
            {
                valid = context.User.HasScope(scopeName);
                if (!valid) break;
            }

            if (valid)
                context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
