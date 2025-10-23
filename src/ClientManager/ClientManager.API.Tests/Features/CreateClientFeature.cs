using ClientManager.Shared.Models;
using Testcontainers.PostgreSql;
using FluentAssertions;
using Moq;
using Testcontainers.RabbitMq;

namespace ClientManager.API.Tests.Features;

[TestFixture]
internal class CreateClientFeature
{
    PostgreSqlContainer _postgresContainer = null!;
    RabbitMqContainer _rabbitMqConatiner = null!;
 
    [Test]
    public async Task When_Creating_A_Valid_Client_The_Service_Should_Enqueue_It_For_Saving()
    {
        // Given a new valid client
        var newClient = new Client
        {
            Id = Guid.Empty,
            FirstName = "Anakin",
            LastName = "Skywalker",
            Email = "Anakin.Skywalker@gmail.com"
        };


    }
}

