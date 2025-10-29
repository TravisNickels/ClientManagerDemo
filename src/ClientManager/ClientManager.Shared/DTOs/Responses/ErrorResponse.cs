namespace ClientManager.Shared.DTOs.Responses;

public class ErrorResponse
{
    public string Message { get; set; } = null!;
    public string? Details { get; set; }
}
