//using AirTicketBooking_Backend.Authentication;
//using AirTicketBooking_Backend.Data;
//using AirTicketBooking_Backend.DTOs;
//using AirTicketBooking_Backend.Models;
//using AirTicketBooking_Backend.Repositories;
//using Microsoft.EntityFrameworkCore;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace AirTicketBooking_Testing
//{
//    [TestFixture]
//    public class BookingServiceTest
//    {
//        private ApplicationDbContext _context;
//        private BookingService _bookingService;

//        private List<Flight> _flights;
//        private List<FlightSeat> _flightSeats;
//        private List<Booking> _bookings;

//        [SetUp]
//        public void Setup()
//        {
//            // Create a unique in-memory database for each test run
//            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString()) // Unique database per test
//                .Options;

//            // Create a new instance of ApplicationDbContext with the in-memory database
//            _context = new ApplicationDbContext(options);

//            // Seed data for flights, flight seats, and bookings
//            _flights = new List<Flight>
//            {
//                new Flight
//                {
//                    FlightId = 1,
//                    AvailableSeats = 5,
//                    PricePerSeat = 100,
//                    FlightNumber = "F123",
//                    Origin = "New York",
//                    Destination = "Los Angeles",
//                    FlightOwnerId = "owner1" // Ensure FlightOwnerId is correctly set
//                }
//            };

//            _flightSeats = new List<FlightSeat>
//            {
//                new FlightSeat { FlightId = 1, SeatNumber = "A1", IsAvailable = true },
//                new FlightSeat { FlightId = 1, SeatNumber = "A2", IsAvailable = true },
//                new FlightSeat { FlightId = 1, SeatNumber = "A3", IsAvailable = true }
//            };

//            _bookings = new List<Booking>
//            {
//                new Booking {
//                    BookingId = 1,
//                    FlightId = 1,
//                    NumberOfSeats = 2,
//                    TotalPrice = 200, // Or calculate dynamically based on seats and flight price
//                    UserId = "user1",
//                    Status = "Confirmed", // Set the status appropriately
//                    BookingDate = DateTime.Now // Ensure booking date is set
//                }
//            };

//            // Seed the data into the in-memory database
//            _context.Flights.AddRange(_flights);
//            _context.FlightSeats.AddRange(_flightSeats);
//            _context.Bookings.AddRange(_bookings);
//            _context.SaveChanges();  // Commit changes to the in-memory database

//            // Initialize the service with the in-memory context
//            _bookingService = new BookingService(_context);
//        }

//        [Test]
//        public async Task BookTicket_ShouldReserveSeats_WhenSeatsAreAvailable()
//        {
//            // Arrange
//            var booking = new Booking
//            {
//                FlightId = 1,
//                NumberOfSeats = 2,
//                UserId = "user2",
//                Status = "Confirmed", // Make sure Status is set
//                BookingDate = DateTime.Now // Ensure you have a valid booking date
//            };

//            // Calculate TotalPrice dynamically
//            booking.TotalPrice = booking.NumberOfSeats * _flights.First().PricePerSeat;

//            // Act
//            var bookingId = await _bookingService.BookTicket(booking);

//            // Assert
//            Assert.AreNotEqual(0, bookingId);
//            var updatedSeats = _context.FlightSeats.Where(fs => fs.FlightId == 1 && !fs.IsAvailable).ToList();
//            Assert.AreEqual(2, updatedSeats.Count);
//            Assert.AreEqual(200, booking.TotalPrice);
//        }

//        [Test]
//        public async Task CancelBooking_ShouldFreeSeats_WhenBookingExists()
//        {
//            // Arrange
//            var bookingId = 1;

//            // Act
//            await _bookingService.CancelBooking(bookingId);

//            // Assert
//            var booking = _context.Bookings.Find(bookingId);
//            Assert.IsNull(booking);  // Booking should be removed

//            var freedSeats = _context.FlightSeats.Where(fs => fs.FlightId == 1 && fs.IsAvailable).ToList();
//            Assert.AreEqual(3, freedSeats.Count);  // All seats should be freed
//        }

//        [Test]
//        public async Task GetBookingHistory_ShouldReturnBookings_ForGivenUser()
//        {
//            // Arrange
//            var userId = "user1";

//            // Act
//            var result = await _bookingService.GetBookingHistory(userId);

//            // Assert
//            Assert.NotNull(result);
//            Assert.That(result.Count(), Is.EqualTo(1));
//            Assert.That(result.First().UserId, Is.EqualTo(userId));
//        }

//        [Test]
//        public async Task ListAllBooking_ShouldReturnBookings_ForFlightOwner()
//        {
//            // Arrange
//            var flightOwnerId = "owner1";  // Ensure this matches the flight owner you're testing for.
//            var userId = "user1";  // The UserId for the booking (simulating the user who made the booking)

//            // Clear any previous data from the database (important for in-memory databases)
//            _context.Flights.RemoveRange(_context.Flights);
//            _context.Bookings.RemoveRange(_context.Bookings);
//            _context.Users.RemoveRange(_context.Users);
//            await _context.SaveChangesAsync();  // Ensure all data is removed

//            // Create a new ApplicationUser with required fields
//            var user = new ApplicationUser
//            {
//                Id = userId,
//                UserName = "testuser",  // Set the UserName or any other required fields
//                Address = "123 Test St",  // Set a default Address (or use any valid string)
//                Gender = "Male"  // Set a default Gender (or any valid value)
//            };

//            _context.Users.Add(user);  // Add the user to the context
//            await _context.SaveChangesAsync();  // Save the user

//            // Create a flight for this owner
//            var flight = new Flight
//            {
//                FlightNumber = "F123",
//                Origin = "New York",
//                Destination = "Los Angeles",
//                DepartureDate = DateTime.Now,
//                AvailableSeats = 5,
//                PricePerSeat = 100,
//                FlightOwnerId = flightOwnerId // Set the FlightOwnerId to simulate ownership
//            };

//            _context.Flights.Add(flight);  // Add the flight to the context
//            await _context.SaveChangesAsync();  // Save the flight

//            // Create a booking for this flight and the given user
//            var booking = new Booking
//            {
//                FlightId = flight.FlightId,   // Link the booking to the flight
//                UserId = userId,  // Set the UserId to simulate a booking by the user
//                NumberOfSeats = 2,
//                TotalPrice = 200,  // NumberOfSeats * PricePerSeat
//                Status = "Confirmed",
//                BookingDate = DateTime.Now
//            };

//            _context.Bookings.Add(booking);  // Add the booking
//            await _context.SaveChangesAsync();  // Save the booking

//            // Act
//            var result = await _bookingService.ListAllBooking(flightOwnerId);  // Call the service method

//            // Assert
//            Assert.NotNull(result);  // Ensure the result is not null
//            Assert.That(result.Count(), Is.EqualTo(1));  // Expecting 1 booking for this flight owner
//            Assert.That(result.First().FlightNumber, Is.EqualTo("F123"));  // Ensure it's the correct flight number
//            Assert.That(result.First().UserName, Is.EqualTo("testuser"));  // Ensure it corresponds to the correct user
//            Assert.That(result.First().Origin, Is.EqualTo("New York"));  // Ensure the origin is correct
//            Assert.That(result.First().Destination, Is.EqualTo("Los Angeles"));  // Ensure the destination is correct
//        }

//        [Test]
//        public async Task ListAllBookingsForAdmin_ShouldReturnAllBookings()
//        {
//            // Arrange
//            var flight = _flights.First();
//            var booking1 = new Booking
//            {
//                BookingId = 2,
//                FlightId = flight.FlightId,
//                NumberOfSeats = 2,
//                TotalPrice = 200,
//                UserId = "user2",
//                Status = "Confirmed",
//                BookingDate = DateTime.Now
//            };

//            var booking2 = new Booking
//            {
//                BookingId = 3,
//                FlightId = flight.FlightId,
//                NumberOfSeats = 1,
//                TotalPrice = 100,
//                UserId = "user3",
//                Status = "Confirmed",
//                BookingDate = DateTime.Now
//            };

//            // Add the bookings to the context and save changes
//            _context.Bookings.AddRange(booking1, booking2);
//            await _context.SaveChangesAsync();

//            // Check if the data is being saved correctly
//            var allBookings = await _context.Bookings.Include(b => b.Flight).Include(b => b.User).ToListAsync();
//            Console.WriteLine($"Bookings count in the database: {allBookings.Count}");  // Debugging line

//            // Act
//            var result = await _bookingService.ListAllBookingsForAdmin();

//            // Output the result to verify if we are getting the expected data
//            Console.WriteLine("Bookings retrieved by the service: " + result.Count());

//            // Assert
//            Assert.NotNull(result);
//            Assert.That(result.Count(), Is.EqualTo(2));  // We expect 2 bookings
//            Assert.That(result.Any(b => b.BookingId == 2), Is.True);  // Ensure booking 2 is returned
//            Assert.That(result.Any(b => b.BookingId == 3), Is.True);  // Ensure booking 3 is returned
//        }


//    }
//}


using AirTicketBooking_Backend.Data;
using AirTicketBooking_Backend.Models;
using AirTicketBooking_Backend.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirTicketBooking_Backend.Tests
{
    [TestFixture]
    public class BookingServiceTests
    {
        private Mock<ApplicationDbContext> _mockContext;
        private BookingService _bookingService;

        [SetUp]
        public void SetUp()
        {
            // Setup mock context with an in-memory database for isolation
            _mockContext = new Mock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
                .Options);

            // Initialize the booking service
            _bookingService = new BookingService(_mockContext.Object);
        }

        [Test]
        public async Task BookTicket_Should_ThrowException_If_FlightNotFound()
        {
            // Arrange
            var booking = new Booking { FlightId = 1, NumberOfSeats = 2 };

            // Mock the Flight DbSet to return null for any FlightId
            _mockContext.Setup(c => c.Flights.FindAsync(It.IsAny<int>()))
                        .ReturnsAsync((Flight)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _bookingService.BookTicket(booking));
            Assert.AreEqual("Flight not found.", exception.Message);
        }

        [Test]
        public async Task BookTicket_Should_ThrowException_If_InsufficientSeats()
        {
            // Arrange
            var flight = new Flight { FlightId = 1, AvailableSeats = 1, PricePerSeat = 100 };
            var booking = new Booking { FlightId = 1, NumberOfSeats = 2 };

            // Mock the Flight DbSet to return the flight
            _mockContext.Setup(c => c.Flights.FindAsync(It.IsAny<int>()))
                .ReturnsAsync(flight);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _bookingService.BookTicket(booking));
            Assert.AreEqual("Insufficient seats available.", exception.Message);
        }

        [Test]
        public async Task GetBookingHistory_Should_Return_BookingsForUser()
        {
            // Arrange
            var userId = "test_user";

            // Prepare test data (bookings for the given user)
            var bookings = new List<Booking>
    {
        new Booking { BookingId = 1, UserId = userId, FlightId = 101, NumberOfSeats = 2 },
        new Booking { BookingId = 2, UserId = userId, FlightId = 102, NumberOfSeats = 3 }
    };

            // Mock the DbSet for Bookings
            _mockContext = new Mock<ApplicationDbContext>();
            _mockContext.SetupDbSet(bookings); // Setup DbSet mock for Bookings

            // Create service
            _bookingService = new BookingService(_mockContext.Object);

            // Act
            var result = await _bookingService.GetBookingHistory(userId);

            // Assert
            Assert.IsNotNull(result);  // Ensure the result is not null
            Assert.AreEqual(2, result.Count());  // Ensure the result contains 2 bookings
            Assert.IsTrue(result.All(b => b.UserId == userId));  // Ensure all bookings belong to the user
        }


        [Test]
        public async Task CancelBooking_Should_ThrowException_If_BookingNotFound()
        {
            // Arrange
            _mockContext.Setup(c => c.Bookings.FindAsync(It.IsAny<int>()))
                        .ReturnsAsync((Booking)null); // Mock that no booking is found

            // Act & Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _bookingService.CancelBooking(1)); // Using bookingId = 1
            Assert.AreEqual("Booking not found.", exception.Message);
        }

        public async Task CancelBooking_Should_MarkSeatsAsAvailable_And_RemoveBooking()
        {
            // Arrange
            var bookingId = 1;
            var flightId = 101;
            var booking = new Booking { BookingId = bookingId, FlightId = flightId };

            var bookingDetails = new List<BookingDetail>
    {
        new BookingDetail { BookingId = bookingId, SeatNumber = "A1" },
        new BookingDetail { BookingId = bookingId, SeatNumber = "A2" }
    };

            var flightSeats = new List<FlightSeat>
    {
        new FlightSeat { FlightId = flightId, SeatNumber = "A1", IsAvailable = false },
        new FlightSeat { FlightId = flightId, SeatNumber = "A2", IsAvailable = false }
    };

            // Mock the DbSet for Bookings, BookingDetails, and FlightSeats
            _mockContext.SetupDbSet(new List<Booking> { booking });  // Mock Bookings DbSet
            _mockContext.SetupDbSet(bookingDetails);  // Mock BookingDetails DbSet
            _mockContext.SetupDbSet(flightSeats);    // Mock FlightSeats DbSet

            // Act
            await _bookingService.CancelBooking(bookingId);

            // Assert
            Assert.IsTrue(flightSeats.All(fs => fs.IsAvailable));  // Ensure seats are marked as available
            _mockContext.Verify(c => c.BookingDetails.RemoveRange(It.IsAny<IEnumerable<BookingDetail>>()), Times.Once);  // Verify removal of booking details
            _mockContext.Verify(c => c.Bookings.Remove(It.IsAny<Booking>()), Times.Once);  // Verify booking removal
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);  // Verify SaveChangesAsync was called
        }

    }
}
