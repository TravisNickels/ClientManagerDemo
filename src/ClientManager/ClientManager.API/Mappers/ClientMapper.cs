using ClientManager.Shared.DTOs.Requests;
using ClientManager.Shared.DTOs.Responses;
using ClientManager.Shared.Models;

namespace ClientManager.API.Mappers;

public static class ClientMapper
{
    public static ClientResponse ToResponse(Client client)
    {
        return new ClientResponse(client.Id, client.FirstName!, client.LastName!, client.Email!);
    }

    public static Client ToEntity(CreateClientRequest request)
    {
        return new Client
        {
            Id = request.Id ?? Guid.Empty,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
        };
    }
}
