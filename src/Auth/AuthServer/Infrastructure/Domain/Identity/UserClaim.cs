using Microsoft.AspNetCore.Identity;

namespace AuthServer.Infrastructure.Domain.Identity;

public class UserClaim : IdentityUserClaim<long>
{
    public virtual User FkUser { get; set; }
}
