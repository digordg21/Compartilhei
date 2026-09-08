using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Exceptions;

namespace Compartilhei.Application.Events.GetEventBySlug;

public sealed class GetEventBySlugHandler
{
    private readonly IEventRepository _eventRepository;

    public GetEventBySlugHandler(
        IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<GetEventBySlugResult> HandleAsync(
        GetEventBySlugQuery query,
        CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetBySlugAsync(
            query.Slug,
            cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException(
                $"Event with slug '{query.Slug}' was not found.");
        }

        return new GetEventBySlugResult(
            eventEntity.Id,
            eventEntity.Name,
            eventEntity.Slug,
            eventEntity.IsActive,
            eventEntity.CoverPhotoId);
    }
}