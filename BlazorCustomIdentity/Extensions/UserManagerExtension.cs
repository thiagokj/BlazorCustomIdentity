using BlazorCustomIdentity.Data;
using Microsoft.AspNetCore.Identity;

namespace BlazorCustomIdentity.Extensions;
public static class UserManagerExtension
{
    public static async Task<IdentityResult> SetUserDataAsync(
            this UserManager<ApplicationUser> userManager,
            ApplicationUser user,
            string firstName,
            string lastName,
            string phoneNumber)
    {
        user.FirstName = firstName;
        user.LastName = lastName;
        user.PhoneNumber = phoneNumber;

        return await userManager.UpdateAsync(user);
    }

    public static async Task<IdentityResult> SetUserPhotoAsync(
    this UserManager<ApplicationUser> userManager,
    ApplicationUser user, string profilePictureBase64)
    {
        user.ProfilePictureBase64 = profilePictureBase64;

        return await userManager.UpdateAsync(user);
    }
}