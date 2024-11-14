using AirTicketBooking_Backend.Models;

namespace AirTicketBooking_Backend.Repositories
{
    public interface IBookingService
    {
        Task<int> BookTicket(Booking booking);
        //Task CancelBooking(int bookingId);
        Task<IEnumerable<Booking>> GetBookingHistory(string userId);
    }
}