using Compartilhei.Application.Abstractions.Identity;
using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Exceptions;
using Compartilhei.Domain.Entities;
using Compartilhei.Domain.Enums;

namespace Compartilhei.Application.Photos.Favorites;

public sealed class FavoritePhotoHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IGuestSessionAccessor _guestSessionAccessor;

    public FavoritePhotoHandler(
        IEventRepository eventRepository,
        IAlbumRepository albumRepository,
        IPhotoRepository photoRepository,
        IFavoriteRepository favoriteRepository,
        IGuestSessionAccessor guestSessionAccessor)
    {
        _eventRepository = eventRepository;
        _albumRepository = albumRepository;
        _photoRepository = photoRepository;
        _favoriteRepository = favoriteRepository;
        _guestSessionAccessor = guestSessionAccessor;
    }

    public async Task<FavoritePhotoResult> HandleAsync(
        FavoritePhotoCommand command,
        CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetBySlugAsync(
            command.EventSlug,
            cancellationToken);

        if (eventEntity is null || !eventEntity.IsActive)
        {
            throw new NotFoundException("Event not found.");
        }

        var album = await _albumRepository.GetActiveByIdAsync(
            command.AlbumId,
            cancellationToken);

        if (album is null || album.EventId != eventEntity.Id)
        {
            throw new NotFoundException("Album not found.");
        }

        var photo = await _photoRepository.GetByIdAsync(
            command.PhotoId,
            cancellationToken);

        if (photo is null ||
            photo.AlbumId != album.Id ||
            photo.Status != PhotoStatus.Available)
        {
            throw new NotFoundException("Photo not found.");
        }

        var guestSessionId = _guestSessionAccessor.GuestSessionId;

        var favorite = await _favoriteRepository
            .GetByPhotoAndGuestSessionAsync(
                photo.Id,
                guestSessionId,
                cancellationToken);

        if (favorite is null)
        {
            await _favoriteRepository.AddAsync(
                new Favorite(photo.Id, guestSessionId),
                cancellationToken);
        }

        var summaries = await _favoriteRepository
            .GetSummariesByPhotoIdsAsync(
                [photo.Id],
                guestSessionId,
                cancellationToken);

        var summary = summaries[photo.Id];

        return new FavoritePhotoResult(
            summary.IsFavorited,
            summary.Count);
    }
}