using Moq;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using AirTicketBooking_Backend.Models;
using AirTicketBooking_Backend.Data;
using AirTicketBooking_Backend.Repositories;
using AirTicketBooking_Backend.Authentication;
using AirTicketBooking_Backend.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirTicketBooking_Testing
{
    [TestFixture]
    public class FlightServiceTests
    {
        private Mock<UserManager<ApplicationUser>> _userManager;

        [SetUp]
        public void Setup()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        private ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Use a unique database for each test
                .Options;
            return new ApplicationDbContext(options);
        }

        [Test]
        public async Task AddFlight_ShouldAddFlightAndGenerateSeats()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var flightService = new FlightService(dbContext, _userManager.Object);

            var flight = new Flight
            {
                FlightId = 1,
                FlightNumber = "F123",
                Origin = "New York",
                Destination = "Los Angeles",
                DepartureDate = DateTime.Now.AddDays(1),
                AvailableSeats = 5,
                PricePerSeat = 100,
                FlightOwnerId = "owner1"
            };

            // Act
            await flightService.AddFlight(flight);

            // Assert
            var savedFlight = await dbContext.Flights.Include(f => f.FlightSeats).FirstOrDefaultAsync(f => f.FlightId == 1);
            Assert.NotNull(savedFlight);
            Assert.AreEqual(flight.FlightNumber, savedFlight.FlightNumber);
            Assert.AreEqual(5, savedFlight.FlightSeats.Count);
        }

        [Test]
        public async Task UpdateFlight_ShouldUpdateFlightDetails()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var flightService = new FlightService(dbContext, _userManager.Object);

            var flight = new Flight
            {
                FlightId = 1,
                FlightNumber = "F123",
                Origin = "New York",
                Destination = "Los Angeles",
                DepartureDate = DateTime.Now.AddDays(1),
                AvailableSeats = 5,
                PricePerSeat = 100,
                FlightOwnerId = "owner1"
            };
            await dbContext.Flights.AddAsync(flight);
            await dbContext.SaveChangesAsync();

            var updatedFlight = new FlightDto
            {
                FlightNumber = "F456",
                Origin = "San Francisco",
                Destination = "Chicago",
                DepartureDate = DateTime.Now.AddDays(2),
                AvailableSeats = 7,
                PricePerSeat = 120
            };

            var mockUser = new ApplicationUser { Id = "owner1" };
            _userManager.Setup(u => u.FindByIdAsync("owner1")).ReturnsAsync(mockUser);
            _userManager.Setup(u => u.GetRolesAsync(mockUser)).ReturnsAsync(new List<string> { "User" });

            // Act
            await flightService.UpdateFlight(1, updatedFlight, "owner1");

            // Assert
            var savedFlight = await dbContext.Flights.Include(f => f.FlightSeats).FirstOrDefaultAsync(f => f.FlightId == 1);
            Assert.NotNull(savedFlight);
            Assert.AreEqual("F456", savedFlight.FlightNumber);
            Assert.AreEqual("San Francisco", savedFlight.Origin);
            Assert.AreEqual("Chicago", savedFlight.Destination);
            Assert.AreEqual(7, savedFlight.FlightSeats.Count);
        }

        [Test]
        public async Task RemoveFlight_ShouldDeleteFlight()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var flightService = new FlightService(dbContext, _userManager.Object);

            var flight = new Flight
            {
                FlightId = 1,
                FlightNumber = "F123",
                Origin = "New York",
                Destination = "Los Angeles",
                DepartureDate = DateTime.Now.AddDays(1),
                AvailableSeats = 5,
                PricePerSeat = 100,
                FlightOwnerId = "owner1",
                FlightSeats = new List<FlightSeat>
                {
                    new FlightSeat { SeatNumber = "A1", IsAvailable = true },
                    new FlightSeat { SeatNumber = "A2", IsAvailable = true }
                }
            };
            await dbContext.Flights.AddAsync(flight);
            await dbContext.SaveChangesAsync();

            // Act
            await flightService.RemoveFlight(flight.FlightId, "owner1", isAdmin: true);

            // Assert
            var result = await dbContext.Flights.FindAsync(flight.FlightId);
            Assert.Null(result);
        }

        [Test]
        public async Task SearchFlights_ShouldReturnFilteredFlights()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var flightService = new FlightService(dbContext, _userManager.Object);

            var flights = new List<Flight>
            {
                new Flight
                {
                    FlightId = 1,
                    FlightNumber = "F123",
                    Origin = "New York",
                    Destination = "Los Angeles",
                    DepartureDate = DateTime.Now.Date,
                    AvailableSeats = 5,
                    PricePerSeat = 100,
                    FlightOwnerId = "owner1"
                },
                new Flight
                {
                    FlightId = 2,
                    FlightNumber = "F456",
                    Origin = "Chicago",
                    Destination = "San Francisco",
                    DepartureDate = DateTime.Now.Date,
                    AvailableSeats = 10,
                    PricePerSeat = 200,
                    FlightOwnerId = "owner2"
                }
            };
            await dbContext.Flights.AddRangeAsync(flights);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await flightService.SearchFlights("New York", "Los Angeles", DateTime.Now.Date);

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("F123", result.First().FlightNumber);
        }

        [Test]
        public async Task GetFlightDetails_ShouldReturnFlightWithSeats()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var flightService = new FlightService(dbContext, _userManager.Object);

            var flight = new Flight
            {
                FlightId = 1,
                FlightNumber = "F123",
                Origin = "New York",
                Destination = "Los Angeles",
                DepartureDate = DateTime.Now.AddDays(1),
                AvailableSeats = 5,
                PricePerSeat = 100,
                FlightOwnerId = "owner1",
                FlightSeats = new List<FlightSeat>
                {
                    new FlightSeat { SeatNumber = "A1", IsAvailable = true },
                    new FlightSeat { SeatNumber = "A2", IsAvailable = true }
                }
            };
            await dbContext.Flights.AddAsync(flight);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await flightService.GetFlightDetails(1);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(1, result.FlightId);
            Assert.AreEqual(2, result.FlightSeats.Count);
        }
    }
}