namespace Compartilhei.Application.Photos.Favorites;

public sealed record FavoritePhotoResult(
    bool IsFavorited,
    int FavoriteCount);