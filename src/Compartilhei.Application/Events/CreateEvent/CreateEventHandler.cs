using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Exceptions;
using Compartilhei.Domain.Entities;
using System.Data;

namespace Compartilhei.Application.Events.CreateEvent;

public sealed class CreateEventHandler
{
    private readonly IEventRepository _eventRepository;

    public CreateEventHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<CreateEventResult> HandleAsync(
        CreateEventCommand command,
        CancellationToken cancellationToken)
    {
        var existingEvent = await _eventRepository.GetBySlugAsync(
            command.Slug,
            cancellationToken);

        if (existingEvent is not null)
        {
            throw new ConflictException(
                $"An event with slug '{command.Slug}' already exists.");
        }

        var eventEntity = new Event(
            command.Name,
            command.Slug);

        await _eventRepository.AddAsync(
            eventEntity,
            cancellationToken);

        return new CreateEventResult(
            eventEntity.Id,
            eventEntity.Name,
            eventEntity.Slug);
    }
}