using System.ComponentModel.DataAnnotations;
using ClientManager.API.Mappers;
using ClientManager.API.Services;
using ClientManager.Shared.Contracts.Commands;
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
            var clientCommand = await _clientService.SendCreateClientMessageAsync(ClientMapper.ToCreateClientCommand(createClientRequest));
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
        catch (ValidationException ex)
        {
            return UnprocessableEntity(new { errors = ex.Message });
        }
    }

    [HttpGet]
    public ActionResult<IEnumerable<ClientResponse>> GetClients()
    {
        var clients = _clientService.GetAllClients()?.Select(ClientMapper.ToResponse).ToList();
        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientResponse>> GetClient(Guid id)
    {
        var client = await _clientService.GetClientByIdAsync(id);

        if (client is null)
            return NotFound($"Client with ID {id} not found");

        return Ok(ClientMapper.ToResponse(client));
    }

    [HttpPatch("archive/{id}")]
    public async Task<IActionResult> ArchiveClient(Guid id)
    {
        await _clientService.SendChangeClientArchiveStatusMessageAsync(ClientMapper.ToUpdateClientArchiveStatusCommand(id, true));
        return Ok();
    }

    [HttpPatch("unarchive/{id}")]
    public async Task<IActionResult> UnArchiveClient(Guid id)
    {
        await _clientService.SendChangeClientArchiveStatusMessageAsync(ClientMapper.ToUpdateClientArchiveStatusCommand(id, false));
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClient([FromBody] UpdateClientRequest updateClientRequest)
    {
        try
        {
            await _clientService.SendUpdateClientMessageAsync(ClientMapper.ToUpdateClientCommand(updateClientRequest));
            return Ok();
        }
        catch (ValidationException ex)
        {
            return UnprocessableEntity(new { errors = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(Guid id)
    {
        await _clientService.SendDeleteClientMessageAsync(ClientMapper.ToDeleteClientCommand(id));
        return Ok();
    }
}
