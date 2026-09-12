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

export function usePhotoUpload() {
  const [photos, setPhotos] = useState<
    UploadPhotoItem[]
  >([]);

  const addFiles = useCallback(
    (files: FileList | File[]) => {
      const selectedFiles = Array.from(files);

      const newPhotos: UploadPhotoItem[] =
        selectedFiles.map((file) => ({
          id: crypto.randomUUID(),
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