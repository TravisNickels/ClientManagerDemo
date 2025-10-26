using ClientManager.API.Controllers;
using ClientManager.API.Services;
using ClientManager.Shared.DTOs.Requests;
using ClientManager.Shared.DTOs.Responses;
using ClientManager.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ClientManager.API.Tests.Unit;

[TestFixture]
internal class ClientControllerTests
{
    Mock<IClientService> mockService = null!;
    ClientController controller = null!;
    CreateClientRequest clientRequest = null!;
    ClientResponse clientResponse = null!;
    Client client = null!;

    [SetUp]
    public void Setup()
    {
        mockService = new Mock<IClientService>();
        controller = new ClientController(mockService.Object);

        clientRequest = new CreateClientRequest(FirstName: "Luke", LastName: "Skywalker", Email: "Luke.Skywalker@gmail.com");
        client = new Client
        {
            Id = Guid.Parse("7d33b8fc-c3b5-45bf-a0d2-d265ee873b91"),
            FirstName = clientRequest.FirstName,
            LastName = clientRequest.LastName,
            Email = clientRequest.Email
        };
        clientResponse = new ClientResponse(Id: client.Id, FirstName: client.FirstName!, LastName: client.LastName!, Email: client.Email!);
    }

    [Test]
    public async Task CreateClient_WhenRequestIsValid_ShouldReturnAccepted()
    {
        // When the service is called with valid client, it returns success
        mockService.Setup(s => s.SendCreateClientMessage(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(client);

        // When calling the client service with the properly mapped client from the client request
        var result = await controller.CreateClient(clientRequest) as AcceptedAtActionResult;

        // Then the result should be Accepted with the expected client with a valid ID
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(202);
        result!.Value.Should().BeEquivalentTo(clientResponse);
        mockService.Verify(s => s.SendCreateClientMessage(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task CreateClient_WhenRequestClientIsNull_ShouldReturnBadRequest()
    {
        // Given a null create client request
        CreateClientRequest? invalidRequest = null;

        // When calling the controller with the null request
        var result = await controller.CreateClient(invalidRequest!) as BadRequestObjectResult;

        // Then it should return BadRequest with appropriate message
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().Be("Client data is required");
    }

    [Test]
    public async Task CreateClient_WhenServiceFails_ShouldReturnInternalServerError()
    {
        // Given the service will throw an exception when trying to send the create client message
        mockService
            .Setup(s => s.SendCreateClientMessage(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Failed", new Exception("Broker down")));

        // When calling the controller to create a client
        var result = await controller.CreateClient(clientRequest) as ObjectResult;

        // Then it should return InvalidOperationException with appropriate message
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(500);

        var innerException = result!.Value as InvalidOperationException;
        innerException.Should().NotBeNull();
        innerException!.Message.Should().Be("Unexpected server error occurred sending message to broker. Failed");
        innerException!.InnerException!.Message.Should().Be("Failed");
        innerException!.InnerException!.InnerException!.Message.Should().Be("Broker down");
    }

    [Test]
    public async Task CreateClient_WhenModelIsInvalid_ShouldReturnBadRequest()
    {
        // Given an invalid model state (missing first name)
        controller.ModelState.AddModelError("FirstName", "First name is required");

        var invalidRequest = new CreateClientRequest(FirstName: "", LastName: "Skywalker", Email: "Luke.Skywalker@gmail.com");

        // When calling the controller to create a client
        var result = await controller.CreateClient(invalidRequest) as BadRequestObjectResult;

        // Then it should return BadRequest with model state errors
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().BeOfType<SerializableError>();
        controller.ModelState.IsValid.Should().BeFalse();
    }
}
