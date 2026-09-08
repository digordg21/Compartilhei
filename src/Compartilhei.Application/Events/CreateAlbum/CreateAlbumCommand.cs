namespace Compartilhei.Application.Events.CreateAlbum;

public sealed record CreateAlbumCommand(
    Guid EventId,
    string Name,
    int DisplayOrder);