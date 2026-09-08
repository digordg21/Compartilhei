namespace Compartilhei.Application.Events.GetEventBySlug;

public sealed record GetEventBySlugResult(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    Guid? CoverPhotoId);