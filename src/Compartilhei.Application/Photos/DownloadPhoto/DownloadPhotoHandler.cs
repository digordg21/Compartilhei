using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Abstractions.Storage;
using Compartilhei.Application.Exceptions;
using Compartilhei.Domain.Enums;

namespace Compartilhei.Application.Photos.DownloadPhoto;

public sealed class DownloadPhotoHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly IPhotoFileStorage _photoFileStorage;

    public DownloadPhotoHandler(
        IEventRepository eventRepository,
        IAlbumRepository albumRepository,
        IPhotoRepository photoRepository,
        IPhotoFileStorage photoFileStorage)
    {
        _eventRepository = eventRepository;
        _albumRepository = albumRepository;
        _photoRepository = photoRepository;
        _photoFileStorage = photoFileStorage;
    }

    public async Task<DownloadPhotoResult> HandleAsync(
        DownloadPhotoQuery query,
        CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetBySlugAsync(
            query.EventSlug,
            cancellationToken);

        if (eventEntity is null || !eventEntity.IsActive)
        {
            throw new NotFoundException("Event not found.");
        }

        var album = await _albumRepository.GetActiveByIdAsync(
            query.AlbumId,
            cancellationToken);

        if (album is null || album.EventId != eventEntity.Id)
        {
            throw new NotFoundException("Album not found.");
        }

        var photo = await _photoRepository.GetByIdAsync(
            query.PhotoId,
            cancellationToken);

        if (photo is null ||
            photo.AlbumId != album.Id ||
            photo.Status != PhotoStatus.Available)
        {
            throw new NotFoundException("Photo not found.");
        }

        if (string.IsNullOrWhiteSpace(photo.OriginalPath))
        {
            throw new NotFoundException("Original photo not found.");
        }

        var content = await _photoFileStorage.OpenReadAsync(
            photo.OriginalPath,
            cancellationToken);

        return new DownloadPhotoResult(
            content,
            photo.FileName,
            GetContentType(photo.FileName));
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName);

        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}