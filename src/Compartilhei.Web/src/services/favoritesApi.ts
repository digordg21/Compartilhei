const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5290";

function buildPhotoUrl(
  eventSlug: string,
  albumId: string,
  photoId: string,
) {
  return `${API_BASE_URL}/api/events/${encodeURIComponent(
    eventSlug,
  )}/albums/${albumId}/photos/${photoId}/favorite`;
}

export type FavoritePhotoResponse = {
  isFavorited: boolean;
  favoriteCount: number;
};

export async function favoritePhoto(
  eventSlug: string,
  albumId: string,
  photoId: string,
): Promise<FavoritePhotoResponse> {
  const response = await fetch(
    buildPhotoUrl(eventSlug, albumId, photoId),
    {
      method: "POST",
      credentials: "include",
    },
  );

  if (!response.ok) {
    throw new Error("Não foi possível favoritar a foto.");
  }

  return response.json();
}

export async function unfavoritePhoto(
  eventSlug: string,
  albumId: string,
  photoId: string,
): Promise<FavoritePhotoResponse> {
  const response = await fetch(
    buildPhotoUrl(eventSlug, albumId, photoId),
    {
      method: "DELETE",
      credentials: "include",
    },
  );

  if (!response.ok) {
    throw new Error("Não foi possível remover o favorito.");
  }

  return response.json();
}

export type FavoritePhotoItem = {
  photoId: string;
  fileName: string;
  thumbnailUrl: string;
  displayUrl: string;
  width: number;
  height: number;
  createdAt: string;
};

export type GetFavoritesResponse = {
  items: FavoritePhotoItem[];
};

export async function getFavorites(
  eventSlug: string,
  albumId: string,
): Promise<GetFavoritesResponse> {
  const url =
    `${API_BASE_URL}/api/events/${encodeURIComponent(eventSlug)}` +
    `/albums/${albumId}/photos/favorites`;

  const response = await fetch(url, {
    method: "GET",
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error("Não foi possível carregar os favoritos.");
  }

  return response.json();
}