using Microsoft.AspNetCore.Identity;

namespace WebApi.Entities;

public class AppRole : IdentityRole<int>
{
    public ICollection<AppUserRole> UserRoles { get; set; } = [];
}
