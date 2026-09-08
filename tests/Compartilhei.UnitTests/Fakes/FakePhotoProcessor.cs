using Compartilhei.Application.Abstractions.Processing;
using Compartilhei.Application.Photos.Processing;

namespace Compartilhei.UnitTests.Fakes;

public sealed class FakePhotoProcessor : IPhotoProcessor
{
    public PhotoProcessingResult Result { get; set; } =
        new(
            "display.jpg",
            "thumbnail.jpg",
            1000,
            800);

    public Exception? Exception { get; set; }

    public int ProcessCallCount { get; private set; }

    public Task<PhotoProcessingResult> ProcessAsync(
        PhotoProcessingRequest request,
        CancellationToken cancellationToken)
    {
        ProcessCallCount++;

        if (Exception is not null)
        {
            throw Exception;
        }

        return Task.FromResult(Result);
    }
}