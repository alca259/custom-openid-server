using Microsoft.AspNetCore.Identity;

namespace AuthServer.Infrastructure.Domain.Identity;

public class UserRole : IdentityUserRole<long>
{
    public virtual User FkUser { get; set; }
    public virtual Role FkRole { get; set; }
}
