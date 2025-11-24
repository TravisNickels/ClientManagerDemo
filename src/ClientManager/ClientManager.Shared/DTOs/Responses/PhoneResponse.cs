namespace ClientManager.Shared.DTOs.Responses;

public record PhoneResponse(Guid Id, Guid ClientId, string PhoneNumber, string PhoneType);
