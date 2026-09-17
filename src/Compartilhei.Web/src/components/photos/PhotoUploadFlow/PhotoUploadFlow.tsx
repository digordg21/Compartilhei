import { useEffect, useRef, useState } from "react";

import "./PhotoUploadFlow.css";

import { PhotoSourceModal } from "../PhotoSourceModal/PhotoSourceModal";
import { PhotoUploadQueue } from "../PhotoUploadQueue/PhotoUploadQueue";

import { usePhotoUpload } from "../../../hooks/usePhotoUpload";

import {
  confirmPhotoUpload,
  requestPhotoUpload,
  uploadPhotoToBlob,
} from "../../../services/photosApi";

import {
  getEventAlbums,
  getEventBySlug,
  type AlbumResponse,
} from "../../../services/eventsApi";

type PhotoUploadFlowProps = {
  eventSlug: string;
  albumId?: string;
  open: boolean;
  onClose: () => void;
  onSuccess?: () => void;
};

export function PhotoUploadFlow({
  eventSlug,
  albumId,
  open,
  onClose,
  onSuccess,
}: PhotoUploadFlowProps) {
  const cameraInputRef = useRef<HTMLInputElement>(null);
  const galleryInputRef = useRef<HTMLInputElement>(null);

  const [albums, setAlbums] = useState<AlbumResponse[]>([]);
  const [selectedAlbumId, setSelectedAlbumId] = useState<string | null>(
    albumId ?? null,
  );

  const [isLoadingAlbums, setIsLoadingAlbums] = useState(false);
  const [albumError, setAlbumError] = useState<string | null>(null);

  const [isPhotoSourceOpen, setIsPhotoSourceOpen] = useState(false);
  const [isPhotoQueueOpen, setIsPhotoQueueOpen] = useState(false);

  const {
    photos,
    addFiles,
    updatePhoto,
    removeFile,
    clear,
  } = usePhotoUpload();

  useEffect(() => {
    if (!open) {
      return;
    }

    if (albumId) {
      setSelectedAlbumId(albumId);
      setIsPhotoSourceOpen(true);
      return;
    }

    let cancelled = false;

    async function loadAlbums() {
      try {
        setIsLoadingAlbums(true);
        setAlbumError(null);

        const event = await getEventBySlug(eventSlug);
        const eventAlbums = await getEventAlbums(event.id);

        if (cancelled) {
          return;
        }

        const activeAlbums = eventAlbums
          .filter((album) => album.isActive)
          .sort(
            (a, b) =>
              a.displayOrder - b.displayOrder,
          );

        setAlbums(activeAlbums);
      } catch (error) {
        if (cancelled) {
          return;
        }

        setAlbumError(
          error instanceof Error
            ? error.message
            : "Não foi possível carregar os álbuns.",
        );
      } finally {
        if (!cancelled) {
          setIsLoadingAlbums(false);
        }
      }
    }

    void loadAlbums();

    return () => {
      cancelled = true;
    };
  }, [open, eventSlug, albumId]);

  const handleAlbumSelect = (id: string) => {
    setSelectedAlbumId(id);
    setAlbumError(null);
    setIsPhotoSourceOpen(true);
  };

  const handleCamera = () => {
    setIsPhotoSourceOpen(false);
    cameraInputRef.current?.click();
  };

  const handleGallery = () => {
    setIsPhotoSourceOpen(false);
    galleryInputRef.current?.click();
  };

  const handleFilesSelected = (
    event: React.ChangeEvent<HTMLInputElement>,
  ) => {
    if (!event.target.files?.length) {
      event.target.value = "";
      return;
    }

    addFiles(event.target.files);
    setIsPhotoQueueOpen(true);

    event.target.value = "";
  };

  const handleUpload = async () => {
    if (!selectedAlbumId) {
      return;
    }

    let hasError = false;

    for (const photo of photos) {
      try {
        updatePhoto(photo.id, {
          status: "Uploading",
          progress: 0,
          error: undefined,
        });

        const authorization = await requestPhotoUpload(
          eventSlug,
          selectedAlbumId,
          photo.file,
        );

        updatePhoto(photo.id, {
          photoId: authorization.photoId,
        });

        await uploadPhotoToBlob(
          authorization.uploadUrl,
          photo.file,
          (progress) => {
            updatePhoto(photo.id, {
              progress,
            });
          },
        );

        updatePhoto(photo.id, {
          status: "Uploaded",
          progress: 100,
        });

        const confirmation = await confirmPhotoUpload(
          eventSlug,
          selectedAlbumId,
          authorization.photoId,
        );

        updatePhoto(photo.id, {
          status:
            confirmation.status === "Uploaded"
              ? "Processing"
              : "Uploaded",
        });
      } catch (error) {
        hasError = true;

        updatePhoto(photo.id, {
          status: "Failed",
          error:
            error instanceof Error
              ? error.message
              : "Erro desconhecido.",
        });
      }
    }

    if (hasError) {
      return;
    }

    setTimeout(() => {
      clear();

      setIsPhotoQueueOpen(false);
      setIsPhotoSourceOpen(false);
      setSelectedAlbumId(albumId ?? null);

      onSuccess?.();
      onClose();
    }, 1500);
  };

  const handleRetry = async (id: string) => {
    const photo = photos.find((item) => item.id === id);

    if (!photo || !selectedAlbumId) {
      return;
    }

    try {
      updatePhoto(photo.id, {
        status: "Uploading",
        progress: 0,
        error: undefined,
      });

      const authorization = await requestPhotoUpload(
        eventSlug,
        selectedAlbumId,
        photo.file,
      );

      updatePhoto(photo.id, {
        photoId: authorization.photoId,
      });

      await uploadPhotoToBlob(
        authorization.uploadUrl,
        photo.file,
        (progress) => {
          updatePhoto(photo.id, {
            progress,
          });
        },
      );

      updatePhoto(photo.id, {
        status: "Uploaded",
        progress: 100,
      });

      const confirmation = await confirmPhotoUpload(
        eventSlug,
        selectedAlbumId,
        authorization.photoId,
      );

      updatePhoto(photo.id, {
        status:
          confirmation.status === "Uploaded"
            ? "Processing"
            : "Uploaded",
      });
    } catch (error) {
      updatePhoto(photo.id, {
        status: "Failed",
        error:
          error instanceof Error
            ? error.message
            : "Erro desconhecido.",
      });
    }
  };

  const handleClose = () => {
    setIsPhotoSourceOpen(false);
    setIsPhotoQueueOpen(false);
    setSelectedAlbumId(albumId ?? null);
    onClose();
  };

  const handleRemoveFile = (id: string) => {
    removeFile(id);

    if (photos.length === 1) {
      handleClose();
    }
  };

  if (!open) {
    return null;
  }

  return (
    <>
      {!selectedAlbumId && (
        <div className="photo-upload-flow__overlay">
          <div className="photo-upload-flow__album-modal">
            <button
              type="button"
              className="photo-upload-flow__close"
              onClick={handleClose}
              aria-label="Fechar"
            >
              ×
            </button>

            <h2 className="photo-upload-flow__title">
              Onde vamos guardar essa foto?
            </h2>

            {isLoadingAlbums && (
              <p className="photo-upload-flow__message">
                Carregando álbuns...
              </p>
            )}

            {albumError && (
              <p className="photo-upload-flow__error">
                {albumError}
              </p>
            )}

            {!isLoadingAlbums && !albumError && (
              <div className="photo-upload-flow__albums">
                {albums.map((album) => (
                  <button
                    key={album.id}
                    type="button"
                    className="photo-upload-flow__album"
                    onClick={() => handleAlbumSelect(album.id)}
                  >
                    {album.name}
                  </button>
                ))}
              </div>
            )}
          </div>
        </div>
      )}

      <input
        ref={cameraInputRef}
        type="file"
        accept="image/jpeg,image/png,image/webp"
        capture="environment"
        hidden
        onChange={handleFilesSelected}
      />

      <input
        ref={galleryInputRef}
        type="file"
        accept="image/jpeg,image/png,image/webp"
        multiple
        hidden
        onChange={handleFilesSelected}
      />

      <PhotoSourceModal
        open={isPhotoSourceOpen}
        onClose={handleClose}
        onCamera={handleCamera}
        onGallery={handleGallery}
      />

      {isPhotoQueueOpen && (
        <PhotoUploadQueue
          photos={photos}
          onRemove={handleRemoveFile}
          onUpload={handleUpload}
          onRetry={handleRetry}
          onTakePhoto={handleCamera}
        />
      )}
    </>
  );
}