import {
  useCallback,
  useState,
} from "react";

export type UploadPhotoStatus =
  | "Pending"
  | "Uploading"
  | "Uploaded"
  | "Processing"
  | "Available"
  | "Failed";

export type UploadPhotoItem = {
  id: string;
  file: File;
  previewUrl: string;
  progress: number;
  status: UploadPhotoStatus;
  error?: string;
  photoId?: string;
};

function createPhotoId(): string {
  // Tenta usar randomUUID quando disponível.
  if (
    typeof crypto !== "undefined" &&
    typeof crypto.randomUUID === "function"
  ) {
    return crypto.randomUUID();
  }

  // Fallback para ambientes móveis/navegadores
  // que não disponibilizam crypto.randomUUID().
  return `${Date.now()}-${Math.random()
    .toString(16)
    .slice(2)}-${Math.random()
    .toString(16)
    .slice(2)}`;
}

export function usePhotoUpload() {
  const [photos, setPhotos] = useState<
    UploadPhotoItem[]
  >([]);

  const addFiles = useCallback(
    (files: FileList | File[]) => {
      const selectedFiles = Array.from(files);

      const newPhotos: UploadPhotoItem[] =
        selectedFiles.map((file) => ({
          id: createPhotoId(),
          file,
          previewUrl:
            URL.createObjectURL(file),
          progress: 0,
          status: "Pending",
        }));

      setPhotos((current) => [
        ...current,
        ...newPhotos,
      ]);
    },
    [],
  );

  const updatePhoto = useCallback(
    (
      id: string,
      update: Partial<UploadPhotoItem>,
    ) => {
      setPhotos((current) =>
        current.map((photo) =>
          photo.id === id
            ? {
                ...photo,
                ...update,
              }
            : photo,
        ),
      );
    },
    [],
  );

  const removeFile = useCallback(
    (id: string) => {
      setPhotos((current) => {
        const photo = current.find(
          (item) => item.id === id,
        );

        if (photo) {
          URL.revokeObjectURL(
            photo.previewUrl,
          );
        }

        return current.filter(
          (item) => item.id !== id,
        );
      });
    },
    [],
  );

  const clear = useCallback(() => {
    setPhotos((current) => {
      current.forEach((photo) => {
        URL.revokeObjectURL(
          photo.previewUrl,
        );
      });

      return [];
    });
  }, []);

  return {
    photos,
    addFiles,
    updatePhoto,
    removeFile,
    clear,
  };
}