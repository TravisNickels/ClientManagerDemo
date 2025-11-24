using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.DTOs.Requests;
using ClientManager.Shared.DTOs.Responses;
using ClientManager.Shared.Models;

namespace ClientManager.API.Mappers;

public static class ClientMapper
{
    public static ClientResponse ToResponse(Client client) =>
        new(client.Id, client.FirstName!, client.LastName!, client.Email!, client.IsArchived!, client.Phones.ConvertAll(phone => phone.ToResponse()));

    public static ClientResponse ToResponse(CreateClient client) =>
        new(client.Id, client.FirstName!, client.LastName!, client.Email!, client.IsArchived!, client.Phones.ConvertAll(phone => phone.ToResponse()));

    public static CreateClient ToCreateClientCommand(CreateClientRequest request)
    {
        return new CreateClient
        {
            Id = request.Id ?? Guid.Empty,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            IsArchived = false,
            Phones = request.Phones != null ? request.Phones.ConvertAll(PhoneMapper.ToCreatePhoneCommand) : []
        };
    }

    public static UpdateClient ToUpdateClientCommand(UpdateClientRequest request)
    {
        return new UpdateClient
        {
            Id = request.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            IsArchived = request.IsArchived,
            Phones = request.Phones != null ? request.Phones.ConvertAll(PhoneMapper.ToUpdatePhoneCommand) : []
        };
    }
}
