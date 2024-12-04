using AirTicketBooking_Backend.DTOs;
using AirTicketBooking_Backend.Models;

namespace AirTicketBooking_Backend.Repositories
{
    public interface IFlightService
    {
        Task AddFlight(Flight flight);
        Task<Flight> GetFlightDetails(int flightId);
        Task RemoveFlight(int flightId, string userId, bool isAdmin);
        Task<IEnumerable<Flight>> SearchFlights(string origin, string destination, DateTime? date);
        Task UpdateFlight(int flightId, FlightDto updatedFlight, string userId);
        Task<IEnumerable<Flight>> GetAllFlights (string ownerId);   //added 

        Task<IEnumerable<Flight>> GetAllFlightsForEveryone();

    }
}