using AirTicketBooking_Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace AirTicketBooking_Backend.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        //additional user information
        public string Address { get; set; }
        public string Gender { get; set; }

        // Navigation properties for relationships with other entities
        public virtual ICollection<Flight> OwnedFlights { get; set; } // For Flight Owners
        public virtual ICollection<Booking> Bookings { get; set; } // For Users
    }
}
