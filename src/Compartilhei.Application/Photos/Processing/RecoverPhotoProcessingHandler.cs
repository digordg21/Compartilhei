using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Abstractions.Processing;
using Compartilhei.Domain.Enums;

namespace Compartilhei.Application.Photos.Processing;

public sealed class RecoverPhotoProcessingHandler
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IPhotoProcessingQueue _processingQueue;

    public RecoverPhotoProcessingHandler(
        IPhotoRepository photoRepository,
        IPhotoProcessingQueue processingQueue)
    {
        _photoRepository = photoRepository;
        _processingQueue = processingQueue;
    }

    public async Task<int> HandleAsync(
        CancellationToken cancellationToken)
    {
        var photos = await _photoRepository.GetAwaitingProcessingAsync(
            cancellationToken);

        foreach (var photo in photos)
        {
            if (photo.Status == PhotoStatus.Processing)
            {
                photo.RequeueForProcessing();

                await _photoRepository.UpdateAsync(
                    photo,
                    cancellationToken);
            }

            await _processingQueue.EnqueueAsync(
                photo.Id,
                cancellationToken);
        }

        return photos.Count;
    }
}