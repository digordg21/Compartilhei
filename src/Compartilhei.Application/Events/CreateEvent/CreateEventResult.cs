namespace Compartilhei.Application.Events.CreateEvent;

public sealed record CreateEventResult(
    Guid Id,
    string Name,
    string Slug);