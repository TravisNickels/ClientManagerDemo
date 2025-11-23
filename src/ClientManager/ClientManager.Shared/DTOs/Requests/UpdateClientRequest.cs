namespace ClientManager.Shared.DTOs.Requests;

public record UpdateClientRequest(Guid Id, string FirstName, string LastName, string Email, bool IsArchived);
