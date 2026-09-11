namespace Compartilhei.Application.Photos.Favorites.GetFavorites;

public sealed record GetFavoritesResult(
    IReadOnlyList<GetFavoritesItemResult> Items);

public sealed record GetFavoritesItemResult(
    Guid PhotoId,
    string FileName,
    string ThumbnailUrl,
    string DisplayUrl,
    int Width,
    int Height,
    DateTimeOffset CreatedAt);