namespace AirTicketBooking_Backend.Models
{
    public class BookingDetail
    {
        public int BookingDetailId { get; set; }

        // Foreign Key to Booking
        public int BookingId { get; set; }
        public virtual Booking Booking { get; set; }

        public string SeatNumber { get; set; }
        public bool IsPaid { get; set; }
    }

}
