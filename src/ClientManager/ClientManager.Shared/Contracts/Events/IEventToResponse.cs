namespace ClientManager.Shared.Contracts.Events;

public interface IEventToResponse<TResponse>
{
    TResponse ToResponse();
}
