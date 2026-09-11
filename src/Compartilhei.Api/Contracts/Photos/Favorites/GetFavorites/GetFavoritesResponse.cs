namespace Compartilhei.Api.Contracts.Photos.Favorites.GetFavorites;

public sealed record GetFavoritesResponse(
    IReadOnlyList<GetFavoritesItemResponse> Items);

public sealed record GetFavoritesItemResponse(
    Guid PhotoId,
    string FileName,
    string ThumbnailUrl,
    string DisplayUrl,
    int Width,
    int Height,
    DateTimeOffset CreatedAt);