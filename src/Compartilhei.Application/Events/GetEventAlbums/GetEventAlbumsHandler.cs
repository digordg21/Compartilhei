using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Exceptions;

namespace Compartilhei.Application.Events.GetEventAlbums;

public sealed class GetEventAlbumsHandler
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IEventRepository _eventRepository;

    public GetEventAlbumsHandler(
        IAlbumRepository albumRepository,
        IEventRepository eventRepository)
    {
        _albumRepository = albumRepository;
        _eventRepository = eventRepository;
    }

    public async Task<IReadOnlyList<GetEventAlbumsResult>> HandleAsync(
        GetEventAlbumsQuery query,
        CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(
            query.EventId,
            cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException(
                $"Event '{query.EventId}' was not found.");
        }

        var albums = await _albumRepository.GetActiveByEventIdAsync(
            query.EventId,
            cancellationToken);

        return albums
            .Select(album => new GetEventAlbumsResult(
                album.Id,
                album.Name,
                album.DisplayOrder,
                album.IsActive))
            .ToList();
    }
}