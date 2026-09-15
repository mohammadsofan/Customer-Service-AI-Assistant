using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIEmployeeSupport.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AIEmployeeSupport.Tests;

public class EventDrivenEmbeddingTests
{
    [Fact]
    public async Task EmbeddingQueue_EnqueueAndDequeue_ReturnsSameScenarioIdImmediately()
    {
        var queue = new EmbeddingQueue();
        var scenarioId = Guid.NewGuid();

        queue.QueueEmbeddingWork(scenarioId);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var dequeuedId = await queue.DequeueAsync(cts.Token);

        dequeuedId.Should().Be(scenarioId);
    }

    [Fact]
    public async Task EmbeddingQueue_MultipleItems_MaintainsFifoOrder()
    {
        var queue = new EmbeddingQueue();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        queue.QueueEmbeddingWork(id1);
        queue.QueueEmbeddingWork(id2);
        queue.QueueEmbeddingWork(id3);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var r1 = await queue.DequeueAsync(cts.Token);
        var r2 = await queue.DequeueAsync(cts.Token);
        var r3 = await queue.DequeueAsync(cts.Token);

        r1.Should().Be(id1);
        r2.Should().Be(id2);
        r3.Should().Be(id3);
    }

    [Fact]
    public async Task EmbeddingQueue_DequeueAsync_RespectsCancellationToken()
    {
        var queue = new EmbeddingQueue();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // pre-canceled

        Func<Task> act = async () => await queue.DequeueAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EmbeddingQueue_HandlesConcurrentEnqueuesSafely()
    {
        var queue = new EmbeddingQueue();
        var count = 50;
        var ids = new List<Guid>();
        for (int i = 0; i < count; i++) ids.Add(Guid.NewGuid());

        // Concurrent producers
        await Task.WhenAll(ids.Select(id => Task.Run(() => queue.QueueEmbeddingWork(id))));

        // Sequential consumption
        var consumed = new List<Guid>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        for (int i = 0; i < count; i++)
        {
            var id = await queue.DequeueAsync(cts.Token);
            consumed.Add(id);
        }

        consumed.Should().HaveCount(count);
        consumed.Should().BeEquivalentTo(ids);
    }
}
