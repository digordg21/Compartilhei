using Compartilhei.Application.Abstractions.Processing;
using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Exceptions;

namespace Compartilhei.Application.Photos.Processing;

public sealed class ProcessPhotoHandler
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IPhotoProcessor _photoProcessor;

    public ProcessPhotoHandler(
        IPhotoRepository photoRepository,
        IPhotoProcessor photoProcessor)
    {
        _photoRepository = photoRepository;
        _photoProcessor = photoProcessor;
    }

    public async Task HandleAsync(
        ProcessPhotoCommand command,
        CancellationToken cancellationToken)
    {
        var photo = await _photoRepository.GetByIdAsync(
            command.PhotoId,
            cancellationToken);

        if (photo is null)
        {
            throw new NotFoundException(
                $"Photo '{command.PhotoId}' was not found.");
        }

        // Somente fotos confirmadas podem entrar no processamento.
        if (photo.Status != Domain.Enums.PhotoStatus.Uploaded)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(photo.OriginalPath))
        {
            throw new BusinessRuleException(
                "Photo does not have an original path.");
        }

        photo.MarkProcessing();

        await _photoRepository.UpdateAsync(
            photo,
            cancellationToken);

        try
        {
            var extension = Path.GetExtension(photo.OriginalPath);

            var originalFileName = $"{photo.Id}{extension}";

            var originalMarker = $"/original/{originalFileName}";

            if (!photo.OriginalPath.EndsWith(
                    originalMarker,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessRuleException(
                    "Photo original path has an invalid format.");
            }

            var basePath = photo.OriginalPath[..^originalMarker.Length];

            var displayPath =
                $"{basePath}/display/{photo.Id}.jpg";

            var thumbnailPath =
                $"{basePath}/thumbnail/{photo.Id}.jpg";

            var result = await _photoProcessor.ProcessAsync(
                new PhotoProcessingRequest(
                    photo.Id,
                    photo.OriginalPath,
                    displayPath,
                    thumbnailPath),
                cancellationToken);


            photo.SetDimensions(
                result.Width,
                result.Height);

            photo.MarkAvailable(
                result.DisplayPath,
                result.ThumbnailPath);

            await _photoRepository.UpdateAsync(
                photo,
                cancellationToken);
        }
        catch
        {
            photo.MarkFailed();

            await _photoRepository.UpdateAsync(
                photo,
                cancellationToken);

            throw;
        }
    }
}