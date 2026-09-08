namespace Compartilhei.Application.Events.CreateAlbum;

public sealed record CreateAlbumResult(
    Guid Id,
    Guid EventId,
    string Name,
    int DisplayOrder,
    bool IsActive);