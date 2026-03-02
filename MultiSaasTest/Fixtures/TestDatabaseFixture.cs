using Infastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;

namespace MultiSaasTest.Fixtures
{
    /// <summary>
    /// Fixture for creating and managing test databases.
    /// Each test gets a fresh in-memory database instance.
    /// </summary>
    public class TestDatabaseFixture : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public ApplicationDbContext Context => _context;

        public TestDatabaseFixture()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
        }
    }
}
