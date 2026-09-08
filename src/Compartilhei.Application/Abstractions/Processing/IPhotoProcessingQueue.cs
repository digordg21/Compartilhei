namespace Compartilhei.Application.Abstractions.Processing;

public interface IPhotoProcessingQueue
{
    ValueTask EnqueueAsync(
        Guid photoId,
        CancellationToken cancellationToken = default);

    ValueTask<Guid> DequeueAsync(
        CancellationToken cancellationToken);
}