using System.Collections.Concurrent;
using Compartilhei.Application.Abstractions.Processing;

namespace Compartilhei.UnitTests.Fakes;

public sealed class FakePhotoProcessingQueue : IPhotoProcessingQueue
{
    private readonly ConcurrentQueue<Guid> _photoIds = new();

    public IReadOnlyCollection<Guid> EnqueuedPhotoIds =>
        _photoIds.ToArray();

    public ValueTask EnqueueAsync(
        Guid photoId,
        CancellationToken cancellationToken = default)
    {
        _photoIds.Enqueue(photoId);

        return ValueTask.CompletedTask;
    }

    public ValueTask<Guid> DequeueAsync(
        CancellationToken cancellationToken)
    {
        if (_photoIds.TryDequeue(out var photoId))
        {
            return ValueTask.FromResult(photoId);
        }

        throw new InvalidOperationException(
            "The processing queue is empty.");
    }
}