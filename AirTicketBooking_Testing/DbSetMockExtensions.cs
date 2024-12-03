using AirTicketBooking_Backend.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;

public static class DbSetMockExtensions
{
    // This method mocks the DbSet to return data properly for common queries.
    public static Mock<DbSet<T>> SetupDbSet<T>(this Mock<ApplicationDbContext> mockContext, List<T> data) where T : class
    {

        var queryableData = data.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();

        // Setup IQueryable properties for mocking LINQ methods.
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

        // Mock the async FindAsync method for retrieving entities by key.
        mockDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((object[] keyValues) => data.FirstOrDefault(x => x.GetType().GetProperty("Id").GetValue(x).Equals(keyValues[0])));

        // Mock Add and Remove methods for DbSet operations (if needed).
        mockDbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(data.Add);
        mockDbSet.Setup(d => d.AddRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(data.AddRange);
        mockDbSet.Setup(d => d.Remove(It.IsAny<T>())).Callback<T>(entity => data.Remove(entity));
        mockDbSet.Setup(d => d.RemoveRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(entities =>
        {
            foreach (var entity in entities)
            {
                data.Remove(entity);
            }
        });

        // Return the mock DbSet when called
        mockContext.Setup(c => c.Set<T>()).Returns(mockDbSet.Object);

        return mockDbSet;
    }
}
