using Microsoft.AspNetCore.Identity;

namespace AuthServer.Infrastructure.Domain.Identity;

public class Role : IdentityRole<long>
{
    public virtual List<UserRole> FkUserRoles { get; set; }
}
