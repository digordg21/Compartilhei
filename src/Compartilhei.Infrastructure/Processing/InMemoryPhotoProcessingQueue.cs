using System.Threading.Channels;
using Compartilhei.Application.Abstractions.Processing;

namespace Compartilhei.Infrastructure.Processing;

public sealed class InMemoryPhotoProcessingQueue : IPhotoProcessingQueue
{
    private readonly Channel<Guid> _queue;

    public InMemoryPhotoProcessingQueue()
    {
        _queue = Channel.CreateUnbounded<Guid>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
    }

    public ValueTask EnqueueAsync(
        Guid photoId,
        CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(
            photoId,
            cancellationToken);
    }

    public ValueTask<Guid> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}