namespace AirTicketBooking_Backend.Models
{
    public class FlightSeat
    {
        public int FlightSeatId { get; set; }

        // Foreign Key to Flight
        public int FlightId { get; set; }
        public virtual Flight Flight { get; set; }

        public string SeatNumber { get; set; }
        public bool IsAvailable { get; set; }
    }

}
