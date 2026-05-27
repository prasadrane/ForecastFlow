using System.Collections.Concurrent;
using ForecastFlow.Core.Interfaces;

namespace ForecastFlow.Api.Data.Repository;

public class InMemoryMessageQueueService : IMessageQueueService
{
    private readonly ConcurrentQueue<object> _queue = new();

    public Task SendMessageAsync<T>(T message)
    {
        if (message != null)
        {
            _queue.Enqueue(message);
        }
        return Task.CompletedTask;
    }

    public Task<T?> ReceiveMessageAsync<T>()
    {
        if (_queue.TryDequeue(out var message))
        {
            return Task.FromResult((T?)message);
        }
        return Task.FromResult(default(T));
    }
}
