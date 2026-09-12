export type EventResponse = {
  id: string;
  name: string;
  slug: string;
  isActive: boolean;
};

export type AlbumResponse = {
  id: string;
  name: string;
  displayOrder: number;
  isActive: boolean;
};

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5290";

async function parseResponse<T>(
  response: Response,
): Promise<T> {
  if (!response.ok) {
    throw new Error(
      `Erro na API (${response.status}).`,
    );
  }

  return response.json() as Promise<T>;
}

export async function getEventBySlug(
  slug: string,
): Promise<EventResponse> {
  const response = await fetch(
    `${API_BASE_URL}/api/events/${encodeURIComponent(slug)}`,
    {
      credentials: "include",
    },
  );

  return parseResponse<EventResponse>(response);
}

export async function getEventAlbums(
  eventId: string,
): Promise<AlbumResponse[]> {
  const response = await fetch(
    `${API_BASE_URL}/api/events/${eventId}/albums`,
    {
      credentials: "include",
    },
  );

  return parseResponse<AlbumResponse[]>(response);
}