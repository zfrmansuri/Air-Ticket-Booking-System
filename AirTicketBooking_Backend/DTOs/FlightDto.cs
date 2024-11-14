namespace AirTicketBooking_Backend.DTOs
{
    public class FlightDto
    {
        //public int FlightId { get; set; }    //we dont want to allow user to enter FlightId
        public string FlightNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureDate { get; set; }
        public int AvailableSeats { get; set; }
        public decimal PricePerSeat { get; set; }
    }
}
