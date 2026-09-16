import type { UploadPhotoItem } from "../../../hooks/usePhotoUpload";
import "./PhotoUploadQueue.css";

type PhotoUploadQueueProps = {
  photos: UploadPhotoItem[];
  onRemove: (id: string) => void;
  onUpload: () => void;
  isUploading?: boolean;
};

function formatFileSize(bytes: number) {
  const megabytes = bytes / (1024 * 1024);

  return `${megabytes.toFixed(1)} MB`;
}

function getStatusText(photo: UploadPhotoItem) {
  switch (photo.status) {
    case "Uploading":
      return `Enviando... ${Math.round(photo.progress)}%`;

    case "Uploaded":
      return "Foto enviada";

    case "Processing":
      return "Processando foto...";

    case "Available":
      return "Foto disponível";

    case "Failed":
      return photo.error ?? "Erro ao enviar foto.";

    default:
      return "Pronta para envio";
  }
}

export function PhotoUploadQueue({
  photos,
  onRemove,
  onUpload,
  isUploading = false,
}: PhotoUploadQueueProps) {
  if (photos.length === 0) {
    return null;
  }

  const hasFailedPhoto = photos.some(
    (photo) => photo.status === "Failed",
  );

  const allUploaded = photos.every(
    (photo) =>
      photo.status === "Uploaded" ||
      photo.status === "Processing" ||
      photo.status === "Available",
  );

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

              <span
                className={`photo-upload-queue__status photo-upload-queue__status--${photo.status.toLowerCase()}`}
              >
                {getStatusText(photo)}
              </span>

              {photo.status === "Uploading" && (
                <div className="photo-upload-queue__progress">
                  <div
                    className="photo-upload-queue__progress-bar"
                    style={{
                      width: `${photo.progress}%`,
                    }}
                  />
                </div>
              )}

            </div>

            {photo.status === "Pending" && (
              <button
                type="button"
                className="photo-upload-queue__remove"
                onClick={() => onRemove(photo.id)}
                aria-label={`Remover ${photo.file.name}`}
              >
                ×
              </button>
            )}

          </article>
        ))}

      </div>

      <p className="photo-upload-queue__count">
        {photos.length}{" "}
        {photos.length === 1
          ? "foto selecionada"
          : "fotos selecionadas"}
      </p>

      {hasFailedPhoto && (
        <div className="photo-upload-queue__message photo-upload-queue__message--error">
          Não foi possível enviar uma ou mais fotos. Verifique os itens acima e tente novamente.
        </div>
      )}

      {allUploaded && !hasFailedPhoto && (
        <div className="photo-upload-queue__message photo-upload-queue__message--success">
          Fotos enviadas com sucesso!
        </div>
      )}

      <button
        type="button"
        className="photo-upload-queue__upload"
        onClick={onUpload}
        disabled={isUploading || allUploaded}
      >
        {isUploading
          ? "Enviando fotos..."
          : allUploaded
            ? "Fotos enviadas"
            : "Enviar fotos"}
      </button>

    </section>
  );
}