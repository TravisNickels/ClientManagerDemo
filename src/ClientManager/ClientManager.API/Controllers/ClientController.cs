using ClientManager.API.Mappers;
using ClientManager.API.Services;
using ClientManager.Shared.DTOs.Requests;
using ClientManager.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client.Exceptions;

namespace ClientManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientController(IClientService clientService) : ControllerBase
{
    readonly IClientService _clientService = clientService;

    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest createClientRequest)
    {
        if (createClientRequest is null)
            return BadRequest("Client data is required");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var clientCommand = await _clientService.SendCreateClientMessage(ClientMapper.ToCreateClientCommand(createClientRequest));
            var response = ClientMapper.ToResponse(clientCommand);
            return AcceptedAtAction(nameof(GetClient), new { id = clientCommand.Id }, response);
        }
        catch (PublishException ex)
        {
            var errorResponse = new ErrorResponse
            {
                Message = "Unexpected server error occurred sending message to broker.",
                Details = ex.InnerException?.Message ?? ex.Message
            };
            return StatusCode(500, errorResponse);
        }
    }

    [HttpGet]
    public ActionResult<IEnumerable<ClientResponse>> GetClients()
    {
        try
        {
            var clients = _clientService.GetAllClients();
            var response = clients.Select(ClientMapper.ToResponse).ToList();
            return (clients is null || !clients.Any()) ? Ok(new List<ClientResponse>()) : Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred while retrieving clients.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClient(Guid id)
    {
        var client = await _clientService.GetClientById(id);
        if (client == null)
            return NotFound();
        return Ok(ClientMapper.ToResponse(client));
    }
}
