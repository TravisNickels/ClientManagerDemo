namespace ClientManager.Shared.DTOs.Responses;

public record ClientResponse(Guid Id, string FirstName, string LastName, string Email, bool IsArchived, List<PhoneResponse> Phones);
