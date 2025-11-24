namespace ClientManager.Shared.DTOs.Requests;

public record CreatePhoneRequest(string PhoneNumber, string PhoneType, Guid? ClientId, Guid? Id);
