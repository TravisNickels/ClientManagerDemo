#nullable enable

namespace ClientManager.Shared.DTOs.Requests;

public record CreateClientRequest(string FirstName, string LastName, string Email, Guid? Id = null);
