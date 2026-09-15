export type PhotoGalleryItem = {
  id: string;
  fileName: string;
  thumbnailUrl: string;
  displayUrl: string;
  width: number;
  height: number;
  createdAt: string;
  isFavorite: boolean;
};

export type PhotoGalleryResponse = {
  items: PhotoGalleryItem[];
  nextCursor: string | null;
  hasMore: boolean;
};

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5290";

export async function getPhotoGallery(
  eventSlug: string,
  albumId: string,
  cursor?: string,
  limit = 30,
): Promise<PhotoGalleryResponse> {
  const params = new URLSearchParams();

  params.set("limit", limit.toString());

  if (cursor) {
    params.set("cursor", cursor);
  }
  // console.log("API BASE URL:", API_BASE_URL);
  // console.log("BUSCANDO GALERIA:", {
  //   eventSlug,
  //   albumId,
  //   cursor,
  //   limit,
  // });

  const response = await fetch(
    `${API_BASE_URL}/api/events/${encodeURIComponent(
      eventSlug,
    )}/albums/${albumId}/photos?${params.toString()}`,
    {
      credentials: "include",
    },
  );

  if (!response.ok) {
    throw new Error(
      `Não foi possível carregar a galeria (${response.status}).`,
    );
  }

  return response.json() as Promise<PhotoGalleryResponse>;
}