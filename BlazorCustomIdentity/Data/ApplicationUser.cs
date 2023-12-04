using Microsoft.AspNetCore.Identity;

namespace BlazorCustomIdentity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int UsernameChangeLimit { get; set; } = 10;
    public string? ProfilePictureBase64 { get; set; }
}

