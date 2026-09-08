using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Exceptions;
using Compartilhei.Domain.Entities;

namespace Compartilhei.Application.Events.CreateAlbum;

public sealed class CreateAlbumHandler
{
    private readonly IAlbumRepository _albumRepository;

    public CreateAlbumHandler(
        IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository;
    }

    public async Task<CreateAlbumResult> HandleAsync(
        CreateAlbumCommand command,
        CancellationToken cancellationToken)
    {
        var eventExists = await _albumRepository.EventExistsAsync(
            command.EventId,
            cancellationToken);

        if (!eventExists)
        {
            throw new NotFoundException(
                $"Event '{command.EventId}' was not found.");
        }

        var albumCount = await _albumRepository.CountByEventIdAsync(
            command.EventId,
            cancellationToken);

        if (albumCount >= 4)
        {
            throw new BusinessRuleException(
                "An event cannot have more than 4 albums.");
        }

        var album = new Album(
            command.EventId,
            command.Name,
            command.DisplayOrder);

        await _albumRepository.AddAsync(
            album,
            cancellationToken);

        return new CreateAlbumResult(
            album.Id,
            album.EventId,
            album.Name,
            album.DisplayOrder,
            album.IsActive);
    }
}