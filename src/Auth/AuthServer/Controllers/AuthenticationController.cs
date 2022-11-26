using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer.Controllers;

public sealed class AuthenticationController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthenticationController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    #region Token
    [HttpPost("/connect/token"), Produces("application/json")]
    public async Task<IActionResult> Exchange(CancellationToken token = default)
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null) throw new InvalidOperationException("OpenID connect request cannot be retrieved");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            return await HandleExchangeCodeGrantType();
        }

        if (request.IsClientCredentialsGrantType())
        {
            return await LoginClientCredentials(request, token);
        }

        return BadRequest(new OpenIddictResponse
        {
            Error = Errors.UnsupportedGrantType
        });
    }

    /// <summary>
    /// Login para aplicaciones SPA o refresco de token
    /// </summary>
    /// <returns></returns>
    private async Task<IActionResult> HandleExchangeCodeGrantType()
    {
        // Retrieve the claims principal stored in the authorization code/device code/refresh token.
        var authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Cannot authenticate."
                }));
        }

        var principal = authenticateResult.Principal;

        // Retrieve the user profile corresponding to the authorization code/refresh token.
        // Note: if you want to automatically invalidate the authorization code/refresh token
        // when the user password/roles change, use the following line instead:
        // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
        var user = await _userManager.GetUserAsync(principal);
        if (user == null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                }));
        }

        // Ensure the user is still allowed to sign in.
        if (!await _signInManager.CanSignInAsync(user))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                }));
        }

        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim, principal));
        }

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Login para otras APIs
    /// </summary>
    /// <param name="request"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<IActionResult> LoginClientCredentials(OpenIddictRequest request, CancellationToken token)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId, token) ??
            throw new InvalidOperationException($"The client {request.ClientId} cannot be found.");

        // Añadir los claims que van a formar parte del token de autenticación (id_token)
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        var subject = await _applicationManager.GetClientIdAsync(application, token);
        identity.SetClaim(type: Claims.Subject, value: subject);

        var displayName = await _applicationManager.GetDisplayNameAsync(application, token);
        identity.SetClaim(type: Claims.Name, value: displayName);

        // Note: In the original OAuth 2.0 specification, the client credentials grant
        // doesn't return an identity token, which is an OpenID Connect concept.
        //
        // As a non-standardized extension, OpenIddict allows returning an id_token
        // to convey information about the client application when the "openid" scope
        // is granted (i.e specified when calling principal.SetScopes()). When the "openid"
        // scope is not explicitly set, no identity token is returned to the client application.

        // Añadimos los scopes permitidos para este cliente
        var principal = new ClaimsPrincipal(identity);

        principal.SetScopes(request.GetScopes());
        var resources = await _scopeManager.ListResourcesAsync(principal.GetScopes(), token).ToListAsync(token);
        principal.SetResources(resources);

        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim, principal));
        }

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Obtiene los token de destino para cada claim
    /// </summary>
    /// <remarks>
    /// Note: by default, claims are NOT automatically included in the access and identity tokens.
    /// To allow OpenIddict to serialize them, you must attach them a destination, that specifies
    /// whether they should be included in access tokens, in identity tokens or in both.
    /// </remarks>
    private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
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
    #endregion
}
