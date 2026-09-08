using Compartilhei.Application.Abstractions.Identity;
using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Abstractions.Processing;
using Compartilhei.Application.Abstractions.Storage;
using Compartilhei.Application.Exceptions;
using Compartilhei.Domain.Enums;

namespace Compartilhei.Application.Photos.ConfirmUpload;

public sealed class ConfirmUploadHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly IPhotoStorage _photoStorage;
    private readonly IGuestSessionAccessor _guestSessionAccessor;
    private readonly IPhotoProcessingQueue _processingQueue;

    public ConfirmUploadHandler(
        IEventRepository eventRepository,
        IAlbumRepository albumRepository,
        IPhotoRepository photoRepository,
        IPhotoStorage photoStorage,
        IGuestSessionAccessor guestSessionAccessor,
        IPhotoProcessingQueue processingQueue)
    {
        _eventRepository = eventRepository;
        _albumRepository = albumRepository;
        _photoRepository = photoRepository;
        _photoStorage = photoStorage;
        _guestSessionAccessor = guestSessionAccessor;
        _processingQueue = processingQueue;
    }

    public async Task<ConfirmUploadResult> HandleAsync(
        ConfirmUploadCommand command,
        CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetBySlugAsync(
            command.EventSlug,
            cancellationToken);

        if (@event is null)
        {
            throw new NotFoundException(
                "Evento não encontrado.");
        }

        var album = await _albumRepository.GetActiveByIdAsync(
            command.AlbumId,
            cancellationToken);

        if (album is null || album.EventId != @event.Id)
        {
            throw new NotFoundException(
                "Álbum não encontrado.");
        }

        var photo = await _photoRepository.GetByIdAsync(
            command.PhotoId,
            cancellationToken);

        if (photo is null || photo.AlbumId != album.Id)
        {
            throw new NotFoundException(
                "Foto não encontrada.");
        }

        if (photo.UploadedBySessionId !=
            _guestSessionAccessor.GuestSessionId)
        {
            throw new NotFoundException(
                "Foto não encontrada.");
        }

        if (photo.Status != PhotoStatus.Pending)
        {
            throw new BusinessRuleException(
                "A foto não está aguardando confirmação de upload.");
        }

        if (string.IsNullOrWhiteSpace(photo.OriginalPath))
        {
            var fileExtension =
                Path.GetExtension(photo.FileName);

            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                throw new BusinessRuleException(
                    "A foto não possui uma extensão válida.");
            }

            var blobPath =
                $"events/{@event.Id}/albums/{album.Id}/original/{photo.Id}{fileExtension.ToLowerInvariant()}";

            var blobExists = await _photoStorage.ExistsAsync(
                blobPath,
                cancellationToken);

            if (!blobExists)
            {
                throw new BusinessRuleException(
                    "O arquivo ainda não foi enviado ao storage.");
            }

            photo.MarkUploaded(blobPath);
        }
        else
        {
            var blobExists = await _photoStorage.ExistsAsync(
                photo.OriginalPath,
                cancellationToken);

            if (!blobExists)
            {
                throw new BusinessRuleException(
                    "O arquivo não foi encontrado no storage.");
            }

            photo.MarkUploaded(photo.OriginalPath);
        }

        await _photoRepository.UpdateAsync(
            photo,
            cancellationToken);

        await _processingQueue.EnqueueAsync(
            photo.Id,
            cancellationToken);

        return new ConfirmUploadResult(
            photo.Id,
            photo.Status.ToString());
    }
}