using AirTicketBooking_Backend.Authentication;

namespace AirTicketBooking_Backend.Models
{
    public class Flight
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureDate { get; set; }
        public int AvailableSeats { get; set; }
        public decimal PricePerSeat { get; set; }

        // Foreign Key to ApplicationUser (FlightOwner)
        public string FlightOwnerId { get; set; }
        public virtual ApplicationUser FlightOwner { get; set; }

        // Navigation property to Booking
        public virtual ICollection<Booking> Bookings { get; set; }

        // Navigation property to FlightSeat
        public virtual ICollection<FlightSeat> FlightSeats { get; set; }
    }

}
