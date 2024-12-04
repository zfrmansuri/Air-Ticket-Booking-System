namespace AirTicketBooking_Backend.DTOs
{
    //public class BookingDto
    //{
    //    public int FlightId { get; set; }
    //    public int NumberOfSeats { get; set; }
    //}

    public class BookingDto
    {
        public int FlightId { get; set; }
        public List<string> SeatIds { get; set; }
    }

}
