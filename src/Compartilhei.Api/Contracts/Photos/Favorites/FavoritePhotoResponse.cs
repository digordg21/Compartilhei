namespace Compartilhei.Api.Contracts.Photos.Favorites;

public sealed record FavoritePhotoResponse(
    bool IsFavorited,
    int FavoriteCount);