using AirTicketBooking_Backend.Authentication;
using AirTicketBooking_Backend.DTOs;
using Microsoft.AspNetCore.Identity;

namespace AirTicketBooking_Backend.Repositories
{
    public interface IUsersAuthenticationService
    {
        Task<object> Login(string email, string password);
        Task<IdentityResult> RegisterFlightOwner(ApplicationUser user, string password);
        Task<IdentityResult> RegisterUser(ApplicationUser user, string password);
        Task EditProfile(string userId, EditProfileDto updatedProfile, string currentUserId);
        Task DeleteProfile(string userId, string currentUserId);

        Task<IEnumerable<ApplicationUser>> GetAllUsersByRole(string role);
    }
}