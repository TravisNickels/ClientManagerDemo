using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.DTOs.Requests;
using ClientManager.Shared.DTOs.Responses;
using ClientManager.Shared.Models;

namespace ClientManager.API.Mappers;

public static class PhoneMapper
{
    public static PhoneResponse ToResponse(this Phone phone) => new(phone.Id, phone.ClientId, phone.Number, phone.Type);

    public static PhoneResponse ToResponse(this CreatePhone phone) => new(phone.Id, phone.ClientId, phone.PhoneNumber, phone.PhoneType);

    public static UpdatePhone ToUpdatePhoneCommand(this UpdatePhoneRequest request)
    {
        return new UpdatePhone
        {
            Id = request.Id,
            ClientId = request.ClientId,
            Number = request.PhoneNumber,
            Type = request.PhoneType
        };
    }

    public static CreatePhone ToCreatePhoneCommand(this CreatePhoneRequest request)
    {
        return new CreatePhone
        {
            Id = request.Id ?? Guid.Empty,
            ClientId = request.ClientId ?? Guid.Empty,
            PhoneNumber = request.PhoneNumber,
            PhoneType = request.PhoneType
        };
    }
}
