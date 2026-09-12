import "./PhotoSourceModal.css";

type PhotoSourceModalProps = {
  open: boolean;
  onClose: () => void;
  onCamera: () => void;
  onGallery: () => void;
};

export function PhotoSourceModal({
  open,
  onClose,
  onCamera,
  onGallery,
}: PhotoSourceModalProps) {
  if (!open) {
    return null;
  }

  return (
    <div
      className="photo-source-modal__overlay"
      onClick={onClose}
      role="presentation"
    >
      <div
        className="photo-source-modal"
        onClick={(event) => event.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby="photo-source-title"
      >
        <h2 id="photo-source-title">
          Como deseja adicionar uma foto?
        </h2>

        <button
          type="button"
          onClick={onCamera}
          className="photo-source-modal__option"
        >
          <span aria-hidden="true">📷</span>
          <span>Tirar foto</span>
        </button>

        <button
          type="button"
          onClick={onGallery}
          className="photo-source-modal__option"
        >
          <span aria-hidden="true">🖼️</span>
          <span>Escolher da galeria</span>
        </button>

        <button
          type="button"
          onClick={onClose}
          className="photo-source-modal__cancel"
        >
          Cancelar
        </button>
      </div>
    </div>
  );
}

