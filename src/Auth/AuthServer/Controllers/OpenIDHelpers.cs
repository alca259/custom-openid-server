using AuthServer.ViewModel;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using System.Security.Claims;
using System.Security.Principal;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer.Controllers;

public static class OpenIDHelpers
{
    /// <summary>
    /// Obtiene los token de destino para cada claim
    /// </summary>
    /// <remarks>
    /// Note: by default, claims are NOT automatically included in the access and identity tokens.
    /// To allow OpenIddict to serialize them, you must attach them a destination, that specifies
    /// whether they should be included in access tokens, in identity tokens or in both.
    /// </remarks>
    public static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
        switch (claim.Type)
        {
            case Claims.Name:
                yield return Destinations.AccessToken;

                if (principal.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;

                if (principal.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;

                if (principal.HasScope(Scopes.Roles))
                    yield return Destinations.IdentityToken;

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp": yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }

    public static IActionResult LoadingPage(this Controller controller, string viewName, string redirectUri)
    {
        controller.HttpContext.Response.StatusCode = 200;
        controller.HttpContext.Response.Headers["Location"] = "";

        return controller.View(viewName, new RedirectViewModel { RedirectUrl = redirectUri });
    }

    /// <summary>
    /// Determines whether this instance is authenticated.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <returns>
    ///   <c>true</c> if the specified principal is authenticated; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsAuthenticated(this IPrincipal principal)
    {
        return principal != null && principal.Identity != null && principal.Identity.IsAuthenticated;
    }

    /// <summary>
    /// Gets the subject identifier.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <returns></returns>
    public static string GetSubjectId(this IPrincipal principal)
    {
        return principal.Identity.GetSubjectId();
    }

    /// <summary>
    /// Gets the subject identifier.
    /// </summary>
    /// <param name="identity">The identity.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">sub claim is missing</exception>
    public static string GetSubjectId(this IIdentity identity)
    {
        var id = identity as ClaimsIdentity;
        var claim = id.FindFirst(OpenIddictConstants.Claims.Subject);

        if (claim == null) throw new InvalidOperationException("sub claim is missing");
        return claim.Value;
    }
}
