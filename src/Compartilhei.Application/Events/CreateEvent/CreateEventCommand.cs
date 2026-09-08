namespace Compartilhei.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
    string Name,
    string Slug);