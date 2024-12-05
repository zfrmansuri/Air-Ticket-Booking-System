using AirTicketBooking_Backend.Data;
using AirTicketBooking_Backend.Models;
using AirTicketBooking_Backend.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
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
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            var context = new ApplicationDbContext(options);
            _mockContext = new Mock<ApplicationDbContext>(options);
            _bookingService = new BookingService(context);
        }

        [Test]
        public async Task BookTicket_Should_ThrowException_If_FlightNotFound()
        {
            // Arrange
            var booking = new Booking { FlightId = 1, NumberOfSeats = 2 };

            _mockContext.Setup(c => c.Flights.FindAsync(It.IsAny<object[]>()))
                        .ReturnsAsync((Flight)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _bookingService.BookTicket(booking, new List<string>()));
            Assert.AreEqual("Flight not found.", exception.Message);
        }

        [Test]
        public async Task BookTicket_Should_ThrowException_If_SeatsUnavailable()
        {
            // Arrange
            var flight = new Flight
            {
                FlightId = 1,
                PricePerSeat = 100,
                FlightSeats = new List<FlightSeat>
        {
            new FlightSeat { SeatNumber = "A1", IsAvailable = false },
            new FlightSeat { SeatNumber = "A2", IsAvailable = false }
        }
            };

            var booking = new Booking { FlightId = 1, NumberOfSeats = 2 };

            // Mock the Flights DbSet to return the flight with unavailable seats
            var flights = new List<Flight> { flight }.AsQueryable();
            var mockFlights = new Mock<DbSet<Flight>>();
            mockFlights.As<IQueryable<Flight>>().Setup(m => m.Provider).Returns(flights.Provider);
            mockFlights.As<IQueryable<Flight>>().Setup(m => m.Expression).Returns(flights.Expression);
            mockFlights.As<IQueryable<Flight>>().Setup(m => m.ElementType).Returns(flights.ElementType);
            mockFlights.As<IQueryable<Flight>>().Setup(m => m.GetEnumerator()).Returns(flights.GetEnumerator());

            _mockContext.Setup(c => c.Flights).Returns(mockFlights.Object);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _bookingService.BookTicket(booking, new List<string> { "A1", "A2" }));
            Assert.AreEqual("One or more requested seats are unavailable or invalid.", exception.Message);
        }


        [Test]
        public async Task GetBookingHistory_Should_Return_BookingsForUser()
        {
            // Arrange
            var userId = "test_user";
            var bookings = new List<Booking>
            {
                new Booking { BookingId = 1, UserId = userId, FlightId = 101, NumberOfSeats = 2 },
                new Booking { BookingId = 2, UserId = userId, FlightId = 102, NumberOfSeats = 3 }
            };

            var mockDbSet = DbSetMockHelper.CreateDbSetMock(bookings);
            _mockContext.Setup(c => c.Bookings).Returns(mockDbSet.Object);

            // Act
            var result = await _bookingService.GetBookingHistory(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(b => b.UserId == userId));
        }

        [Test]
        public async Task CancelBooking_Should_ThrowException_If_BookingNotFound()
        {
            // Arrange
            _mockContext.Setup(c => c.Bookings.FindAsync(It.IsAny<int>()))
                        .ReturnsAsync((Booking)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _bookingService.CancelBooking(1));
            Assert.AreEqual("Booking not found.", exception.Message);
        }

        [Test]
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

            var bookingDbSetMock = DbSetMockHelper.CreateDbSetMock(new List<Booking> { booking });
            var bookingDetailsDbSetMock = DbSetMockHelper.CreateDbSetMock(bookingDetails);
            var flightSeatsDbSetMock = DbSetMockHelper.CreateDbSetMock(flightSeats);

            _mockContext.Setup(c => c.Bookings).Returns(bookingDbSetMock.Object);
            _mockContext.Setup(c => c.BookingDetails).Returns(bookingDetailsDbSetMock.Object);
            _mockContext.Setup(c => c.FlightSeats).Returns(flightSeatsDbSetMock.Object);

            // Act
            await _bookingService.CancelBooking(bookingId);

            // Assert
            Assert.IsTrue(flightSeats.All(fs => fs.IsAvailable));
            bookingDetailsDbSetMock.Verify(b => b.RemoveRange(It.IsAny<IEnumerable<BookingDetail>>()), Times.Once);
            bookingDbSetMock.Verify(b => b.Remove(It.IsAny<Booking>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }

    public static class DbSetMockHelper
    {
        public static Mock<DbSet<T>> CreateDbSetMock<T>(IEnumerable<T> elements) where T : class
        {
            var queryable = elements.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();

            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return dbSetMock;
        }
    }
}
