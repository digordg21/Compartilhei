namespace Compartilhei.Application.Events.GetEventAlbums;

public sealed record GetEventAlbumsResult(
    Guid Id,
    string Name,
    int DisplayOrder,
    bool IsActive);