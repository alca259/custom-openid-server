using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using System.Security.Claims;

namespace AuthServer.Controllers;

public sealed class AuthenticationController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthenticationController(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("/connect/token")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Tokenizame()
    {
        var oidcRequest = HttpContext.GetOpenIddictServerRequest();
        if (oidcRequest == null) throw new InvalidOperationException("OpenID connect request cannot be retrieved");

        if (oidcRequest.IsPasswordGrantType())
            return await TokensForPasswordGrantType(oidcRequest);

        // TODO: Diferentes autenticaciones (?)
        if (oidcRequest.IsRefreshTokenGrantType())
        {
            // return tokens for refresh token flow
        }

        return BadRequest(new OpenIddictResponse
        {
            Error = OpenIddictConstants.Errors.UnsupportedGrantType
        });
    }

    /// <summary>
    /// TODO: Extrapolar a un servicio/mediator/etc
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<IActionResult> TokensForPasswordGrantType(OpenIddictRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
            return Unauthorized();

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!signInResult.Succeeded)
            return Unauthorized();

        var claims = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        claims.AddClaim(OpenIddictConstants.Claims.Subject, user.Id);
        claims.AddClaim(OpenIddictConstants.Claims.Username, user.UserName);
        claims.AddClaim(OpenIddictConstants.Claims.Email, user.Email);

        foreach (var userRole in user.FkUserRoles)
        {
            claims.AddClaim(OpenIddictConstants.Claims.Role, userRole.FkRole.NormalizedName, OpenIddictConstants.Destinations.AccessToken);
        }

        var claimsPrincipal = new ClaimsPrincipal(claims);
        claimsPrincipal.SetScopes(new string[]
        {
            OpenIddictConstants.Scopes.Roles,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.OfflineAccess // TODO: Check docs ????
        });

        return SignIn(claimsPrincipal);
    }
}
