using Microsoft.AspNetCore.Identity;

namespace AuthServer.Infrastructure.Domain.Identity;

public class User : IdentityUser<long>
{
    public virtual List<UserClaim> FkUserClaims { get; set; }
    public virtual List<UserRole> FkUserRoles { get; set; }
    public virtual List<UserToken> FkUserTokens { get; set; }
}
