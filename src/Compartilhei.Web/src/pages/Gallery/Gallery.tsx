import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

import "./Gallery.css";

import { getPhotoGallery, type PhotoGalleryItem } from "../../services/galleryApi";

export default function Gallery() {
  console.log("✅ GALLERY FOI MONTADA");
  const navigate = useNavigate();
  const { eventSlug, albumId } = useParams();

  const [photos, setPhotos] = useState<
    PhotoGalleryItem[]
  >([]);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!eventSlug || !albumId) {
      setError("Álbum não informado.");
      setLoading(false);
      return;
    }

    let cancelled = false;

    async function loadGallery() {
      if (!eventSlug || !albumId) {
        return;
      }

      try {
        setLoading(true);
        setError(null);

        const result = await getPhotoGallery(
          eventSlug,
          albumId,
        );

        if (cancelled) {
          return;
        }

        setPhotos(result.items);
      } catch (err) {
        if (cancelled) {
          return;
        }

        setError(
          err instanceof Error
            ? err.message
            : "Não foi possível carregar a galeria.",
        );
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadGallery();

    return () => {
      cancelled = true;
    };
  }, [eventSlug, albumId]);

  const goBack = () => {
    if (!eventSlug) {
      return;
    }

    navigate(`/${eventSlug}/albuns`);
  };

  if (loading) {
    return (
      <main className="gallery">
        <div className="gallery__state">
          Carregando fotos...
        </div>
      </main>
    );
  }

  if (error) {
    return (
      <main className="gallery">
        <div className="gallery__state gallery__state--error">
          {error}
        </div>

        <button
          type="button"
          className="gallery__back"
          onClick={goBack}
        >
          Voltar
        </button>
      </main>
    );
  }

  return (
    <main className="gallery">
      <header className="gallery__header">
        <button
          type="button"
          className="gallery__back"
          onClick={goBack}
          aria-label="Voltar para álbuns"
        >
          ←
        </button>

        <h1>Fotos</h1>
      </header>

      {photos.length === 0 ? (
        <div className="gallery__state">
          Ainda não existem fotos neste álbum.
        </div>
      ) : (
        <section className="gallery__grid">
          {photos.map((photo) => (
            <button
              type="button"
              className="gallery__photo"
              key={photo.id}
            >
              <img
                src={photo.thumbnailUrl}
                alt={photo.fileName}
                loading="lazy"
              />
            </button>
          ))}
        </section>
      )}
    </main>
  );
}