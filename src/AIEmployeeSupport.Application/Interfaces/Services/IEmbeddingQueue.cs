using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IEmbeddingQueue
{
    void QueueEmbeddingWork(Guid scenarioId);
    ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken);
}
