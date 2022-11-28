using Microsoft.AspNetCore.Identity;

namespace AuthServer.Infrastructure.Domain.Identity;

public class UserToken : IdentityUserToken<long>
{
    public virtual User FkUser { get; set; }
}
