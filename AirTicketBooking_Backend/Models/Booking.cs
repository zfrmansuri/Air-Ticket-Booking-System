using AirTicketBooking_Backend.Authentication;
using System.ComponentModel.DataAnnotations;

namespace AirTicketBooking_Backend.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        // Foreign Key to ApplicationUser (User)
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        // Foreign Key to Flight
        public int FlightId { get; set; }
        public virtual Flight Flight { get; set; }

        public DateTime BookingDate { get; set; }
        public int NumberOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
        //public BookingStatus Status { get; set; }

        [RegularExpression("^(Confirmed|Canceled|Pending)$", ErrorMessage = "Invalid status. Allowed values are: Confirmed, Canceled, Pending.")]
        public string Status { get; set; }

        // Navigation property to BookingDetails
        public virtual ICollection<BookingDetail> BookingDetails { get; set; }
    }

}
