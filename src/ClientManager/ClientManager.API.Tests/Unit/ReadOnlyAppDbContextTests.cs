using ClientManager.Shared.Data;
using ClientManager.Shared.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ClientManager.API.Tests.Unit;

[TestFixture]
internal class ReadOnlyAppDbContextTests
{
    ReadOnlyAppDbContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ReadOnlyAppDbContext>().UseInMemoryDatabase("ReadOnlyTestDb").Options;

        _context = new ReadOnlyAppDbContext(options);
    }

    [TearDown]
    public void Dispose()
    {
        if (_context != null)
            _context.Dispose();
    }

    [Test]
    public void Add_ShouldThrowInvalidOperationException()
    {
        var client = new Client { FirstName = "Luke", LastName = "Skywalker" };

        Action act = () => _context.Add(client);

        act.Should().Throw<InvalidOperationException>().WithMessage("*read-only*");
    }

    [Test]
    public async Task AddAsync_ShouldThrowInvalidOperationException()
    {
        var client = new Client { FirstName = "Leia", LastName = "Oragna" };

        Func<Task> act = async () => await _context.AddAsync(client);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*read-only*");
    }

    [Test]
    public void Update_ShouldThrowInvalidOperationException()
    {
        var client = new Client { FirstName = "Han", LastName = "Solo" };

        Action act = () => _context.Update(client);

        act.Should().Throw<InvalidOperationException>().WithMessage("*read-only*");
    }

    [Test]
    public void Remove_ShouldThrowInvalidOperationException()
    {
        var client = new Client { FirstName = "Ashoka", LastName = "Tano" };

        Action act = () => _context.Remove(client);

        act.Should().Throw<InvalidOperationException>().WithMessage("*read-only*");
    }

    [Test]
    public void SaveChanges_ShouldThrowInvalidOperationException()
    {
        Action act = () => _context.SaveChanges();

        act.Should().Throw<InvalidOperationException>().WithMessage("*read-only*");
    }

    [Test]
    public async Task SaveChangesAsync_ShouldThrowInvalidOperationException()
    {
        Func<Task> act = async () => await _context.SaveChangesAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*read-only*");
    }
}
