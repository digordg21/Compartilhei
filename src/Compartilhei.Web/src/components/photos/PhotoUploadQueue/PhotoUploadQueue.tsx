import type { UploadPhotoItem } from "../../../hooks/usePhotoUpload";
import "./PhotoUploadQueue.css";

type PhotoUploadQueueProps = {
  photos: UploadPhotoItem[];
  onRemove: (id: string) => void;
  onUpload: () => void;
};

function formatFileSize(bytes: number) {
  const megabytes = bytes / (1024 * 1024);

  return `${megabytes.toFixed(1)} MB`;
}

export function PhotoUploadQueue({
  photos,
  onRemove,
  onUpload,
}: PhotoUploadQueueProps) {
  if (photos.length === 0) {
    return null;
  }

  return (
    <section className="photo-upload-queue">
      <h2 className="photo-upload-queue__title">
        Fotos selecionadas
      </h2>

      <div className="photo-upload-queue__grid">
        {photos.map((photo) => (
          <article
            key={photo.id}
            className="photo-upload-queue__item"
          >
            <img
              src={photo.previewUrl}
              alt={photo.file.name}
              className="photo-upload-queue__preview"
            />

            <div className="photo-upload-queue__info">
              <span className="photo-upload-queue__name">
                {photo.file.name}
              </span>

              <span className="photo-upload-queue__size">
                {formatFileSize(photo.file.size)}
              </span>
            </div>

            <button
              type="button"
              className="photo-upload-queue__remove"
              onClick={() => onRemove(photo.id)}
              aria-label={`Remover ${photo.file.name}`}
            >
              ×
            </button>
          </article>
        ))}
      </div>

      <p className="photo-upload-queue__count">
        {photos.length}{" "}
        {photos.length === 1 ? "foto selecionada" : "fotos selecionadas"}
      </p>

      <button
        type="button"
        className="photo-upload-queue__upload"
        onClick={onUpload}
      >
        Enviar fotos
      </button>
    </section>
  );
}