export type RequestUploadResponse = {
  photoId: string;
  blobPath: string;
  uploadUrl: string;
};

export type ConfirmUploadResponse = {
  photoId: string;
  status: string;
};

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5290";

function buildPhotosUrl(
  eventSlug: string,
  albumId: string,
) {
  return `${API_BASE_URL}/api/events/${encodeURIComponent(
    eventSlug,
  )}/albums/${albumId}/photos`;
}

export async function requestPhotoUpload(
  eventSlug: string,
  albumId: string,
  file: File,
): Promise<RequestUploadResponse> {
  const response = await fetch(
    `${buildPhotosUrl(eventSlug, albumId)}/upload-request`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      credentials: "include",
      body: JSON.stringify({
        fileName: file.name,
        fileSize: file.size,
        contentType: file.type,
      }),
    },
  );

  if (!response.ok) {
    throw new Error(
      `Não foi possível autorizar o upload (${response.status}).`,
    );
  }

  return response.json();
}

export async function uploadPhotoToBlob(
  uploadUrl: string,
  file: File,
  onProgress?: (progress: number) => void,
): Promise<void> {
  await new Promise<void>((resolve, reject) => {
    const xhr = new XMLHttpRequest();

    xhr.open("PUT", uploadUrl);

    xhr.setRequestHeader(
      "x-ms-blob-type",
      "BlockBlob",
    );

    xhr.upload.onprogress = (event) => {
      if (!event.lengthComputable) {
        return;
      }

      const progress =
        (event.loaded / event.total) * 100;

      onProgress?.(progress);
    };

    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) {
        onProgress?.(100);
        resolve();
        return;
      }

      reject(
        new Error(
          `Upload do arquivo falhou (${xhr.status}).`,
        ),
      );
    };

    xhr.onerror = () => {
      reject(
        new Error("Falha de comunicação com o Storage."),
      );
    };

    xhr.onabort = () => {
      reject(
        new Error("Upload cancelado."),
      );
    };

    xhr.send(file);
  });
}

export async function confirmPhotoUpload(
  eventSlug: string,
  albumId: string,
  photoId: string,
): Promise<ConfirmUploadResponse> {
  const response = await fetch(
    `${buildPhotosUrl(
      eventSlug,
      albumId,
    )}/${photoId}/confirm-upload`,
    {
      method: "POST",
      credentials: "include",
    },
  );

  if (!response.ok) {
    throw new Error(
      `Não foi possível confirmar o upload (${response.status}).`,
    );
  }

  return response.json();
}