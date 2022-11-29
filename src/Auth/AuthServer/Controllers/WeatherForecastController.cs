using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer.Controllers;

[ApiController]
[Route("weather")]
public class WeatherForecastController : ControllerBase
{
    private readonly IOpenIddictApplicationManager _applicationManager;

    public WeatherForecastController(
        IOpenIddictApplicationManager applicationManager)
    {
        _applicationManager = applicationManager;
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("message")]
    public async Task<IActionResult> GetMessage()
    {
        var sub = User.FindFirstValue(Claims.Subject);
        if (string.IsNullOrEmpty(sub))
            return BadRequest();

        var client = await _applicationManager.FindByClientIdAsync(sub);
        if (client == null)
            return BadRequest();

        return Content($"{await _applicationManager.GetDisplayNameAsync(client)} has been successfully authenticated.");
    }
}