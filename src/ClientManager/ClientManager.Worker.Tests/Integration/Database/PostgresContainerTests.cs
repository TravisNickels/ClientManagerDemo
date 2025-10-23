using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using ClientManager.Worker.Data;
using FluentAssertions;

namespace ClientManager.Worker.Tests.Integration.Database;

[TestFixture]
internal class PostgresContainerTests
{
    PostgreSqlContainer _container = null!;
    protected AppDbContext _dbContext = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("clientManagerTestDb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _container.StartAsync();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
        
        _dbContext = new AppDbContext(options);
        
        // Ensure database is created - bypasses migrations for testing
        await _dbContext.Database.EnsureCreatedAsync();
    }

    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        if(_container is not null)
            await _container.DisposeAsync();
        if(_dbContext is not null)
            await _dbContext.DisposeAsync();
    }

    [Test]
    public async Task TestDatabaseConnection()
    {
        // Act
        var canConnect = await _dbContext.Database.CanConnectAsync();
        // Assert
        canConnect.Should().BeTrue();
    }
}

