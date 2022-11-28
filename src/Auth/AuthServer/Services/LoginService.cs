using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthServer.Services;

public class LoginService : ILoginService<User>
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginService> _logger;

    public LoginService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        ILogger<LoginService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<User> FindByUserId(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<User> FindByUsername(string user)
    {
        _logger.LogDebug($"user {user}");
        return await _userManager.FindByNameAsync(user);
    }

    public async Task<bool> ValidateCredentials(User user, string password)
    {
        var IsValidateCredentials = await _userManager.CheckPasswordAsync(user, password);

        _logger.LogDebug($"ValidateCredentials User {user}  IsValidateCredentials {IsValidateCredentials}");

        return IsValidateCredentials;
    }

    public Task SignIn(User user)
    {
        _logger.LogDebug($"SignIn User {user.UserName} ");
        return _signInManager.SignInAsync(user, true);
    }

    public Task SignInAsync(User user, string returnUrl, bool rememberLogin,
        IEnumerable<Claim> aditionalClaims = null)
    {
        _logger.LogDebug($"SignIn User {user.UserName}  returnUrl {returnUrl}  rememberLogin {rememberLogin} {{aditionalClaims}}", aditionalClaims);

        var cookieLife = _configuration.GetValue<double?>("AccountOptions:CookieLifetime");
        var props = new AuthenticationProperties
        {
            IsPersistent = cookieLife.HasValue,
            AllowRefresh = true,
            RedirectUri = returnUrl
        };

        if (_configuration.GetValue<bool>("AccountOptions:AllowRememberLogin", false)
            && rememberLogin)
        {
            var PermanentCookieLifetimeDays = _configuration.GetValue("AccountOptions:PermanentCookieLifetimeDays", 365);

            props.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(PermanentCookieLifetimeDays);
            props.IsPersistent = true;
        }

        return aditionalClaims != null ?
            _signInManager.SignInWithClaimsAsync(user, props, aditionalClaims) :
            _signInManager.SignInAsync(user, props);
    }

    public Task SignOutAsync()
    {
        _logger.LogDebug($"SignOutAsync");

        return _signInManager.SignOutAsync();
    }
}
