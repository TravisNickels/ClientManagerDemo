namespace ClientManager.Shared.DTOs.Requests;

public record UpdatePhoneRequest(Guid Id, Guid ClientId, string PhoneNumber, string PhoneType);
