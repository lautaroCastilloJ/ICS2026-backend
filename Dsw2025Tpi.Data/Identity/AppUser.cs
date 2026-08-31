using Microsoft.AspNetCore.Identity;
using Dsw2025Tpi.Data.Identity;


namespace Dsw2025Tpi.Data.Identity;

public class AppUser : IdentityUser 
{
    public string? DisplayName { get; set; }
    public Guid? CustomerId { get; set; }


}
