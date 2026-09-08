using Compartilhei.Application.Photos.Processing;

namespace Compartilhei.Application.Abstractions.Processing;

public interface IPhotoProcessor
{
    Task<PhotoProcessingResult> ProcessAsync(
        PhotoProcessingRequest request,
        CancellationToken cancellationToken);
}