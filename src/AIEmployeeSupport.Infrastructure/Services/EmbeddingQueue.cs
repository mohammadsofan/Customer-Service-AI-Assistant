using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AIEmployeeSupport.Application.Interfaces.Services;

namespace AIEmployeeSupport.Infrastructure.Services;

public class EmbeddingQueue : IEmbeddingQueue
{
    private readonly Channel<Guid> _channel;

    public EmbeddingQueue()
    {
        var options = new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateUnbounded<Guid>(options);
    }

    public void QueueEmbeddingWork(Guid scenarioId)
    {
        _channel.Writer.TryWrite(scenarioId);
    }

    public ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}
