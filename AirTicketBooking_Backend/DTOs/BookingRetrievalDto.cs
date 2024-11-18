namespace AirTicketBooking_Backend.DTOs
{
    public class BookingRetrievalDto
    {
        public int BookingId { get; set; }
        public DateTime BookingDate { get; set; }
        public int NumberOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string FlightNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string UserName { get; set; }   // Add UserName to show in the response
    }

}
