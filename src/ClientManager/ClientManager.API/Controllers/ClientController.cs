using ClientManager.API.Mappers;
using ClientManager.API.Services;
using ClientManager.Shared.DTOs.Requests;
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
            var client = await _clientService.SendCreateClientMessage(ClientMapper.ToEntity(createClientRequest));
            return AcceptedAtAction(nameof(GetClient), new { id = client.Id }, ClientMapper.ToResponse(client));
        }
        catch (PublishException ex)
        {
            return StatusCode(500, new { message = "Unexpected server error occurred sending message to broker.", details = ex.InnerException?.Message ?? ex.Message });
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
