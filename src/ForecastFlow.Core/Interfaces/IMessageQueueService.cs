namespace ForecastFlow.Core.Interfaces;

public interface IMessageQueueService
{
    Task SendMessageAsync<T>(T message);
    Task<T?> ReceiveMessageAsync<T>();
}
