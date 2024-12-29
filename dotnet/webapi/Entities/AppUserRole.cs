using Microsoft.AspNetCore.Identity;

namespace WebApi.Entities;

public class AppUserRole : IdentityUserRole<int>
{
    public User User { get; set; } = null!;
    public AppRole Role { get; set; } = null!;
}
