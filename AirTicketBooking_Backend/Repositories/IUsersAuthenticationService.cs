using AirTicketBooking_Backend.Authentication;
using Microsoft.AspNetCore.Identity;

namespace AirTicketBooking_Backend.Repositories
{
    public interface IUsersAuthenticationService
    {
        Task<string> Login(string email, string password);
        Task<IdentityResult> RegisterFlightOwner(ApplicationUser user, string password);
        Task<IdentityResult> RegisterUser(ApplicationUser user, string password);
    }
}