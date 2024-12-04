using AirTicketBooking_Backend.DTOs;
using AirTicketBooking_Backend.Models;

namespace AirTicketBooking_Backend.Repositories
{
    public interface IBookingService
    {

        //Task<int> BookTicket(Booking booking);

        Task<int> BookTicket(Booking booking, List<string> seatIds);
        Task<IEnumerable<Booking>> GetBookingHistory(string userId);
        Task CancelBooking(int bookingId);
        Task<IEnumerable<BookingRetrievalDto>> ListAllBooking(string flightOwnerId);
        Task<IEnumerable<BookingRetrievalDto>> ListAllBookingsForAdmin();
    }
}