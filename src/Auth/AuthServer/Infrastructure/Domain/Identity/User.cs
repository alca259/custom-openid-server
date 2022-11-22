using Microsoft.AspNetCore.Identity;

namespace AuthServer.Infrastructure.Domain.Identity;

public class User : IdentityUser<long>
{
    public virtual List<UserRole> FkUserRoles { get; set; }
}
