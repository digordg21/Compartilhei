import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

import "./Favorites.css";

import {
  getEventAlbums,
  getEventBySlug,
} from "../../services/eventsApi";

import {
  getFavorites,
  unfavoritePhoto,
  downloadFavorites,
  type FavoritePhotoItem,
} from "../../services/favoritesApi";

import fotoTopo from "../../assets/images/whats-app-x-0020-image-x-0020-2026-08-30-x-0020-at-x-0020-17-31-08-jpeg0.png";
import fundoBranco from "../../assets/decorations/fundo-branco0.svg";
import botaoInicio from "../../assets/icons/bot-o-de-inicio0.svg";
import tituloFavoritos from "../../assets/decorations/TEXTO FAVORITOS.svg";
import voltar from "../../assets/festa/voltar0.svg";
import tituloFotos from "../../assets/festa/texto-fotos0.svg";

import iconeDownload from "../../assets/icons/download-galerias.svg";
import textoDownload from "../../assets/decorations/TEXTO DOWNLOAD 2.svg";
import textoBaixeFotos from "../../assets/decorations/TEXTO DOWNLOAD ALBÚM 2.svg";

import favoritoIcon from "../../assets/decorations/BOTÃO CORAÇÃO vermelho svg.svg";

type FavoriteItem = FavoritePhotoItem & {
  albumId: string;
};

function Favorites() {
  const navigate = useNavigate();
  const { eventSlug } = useParams();

  const [favorites, setFavorites] = useState<FavoriteItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [favoriteLoadingId, setFavoriteLoadingId] =
    useState<string | null>(null);

  const [downloadingFavorites, setDownloadingFavorites] =
    useState(false);

  const [selectedPhotoIndex, setSelectedPhotoIndex] =
    useState<number | null>(null);

  useEffect(() => {
    if (!eventSlug) {
      setError("Evento não informado.");
      setLoading(false);
      return;
    }

    let cancelled = false;

    async function loadFavorites() {
      try {
        setLoading(true);
        setError(null);

        const event = await getEventBySlug(eventSlug!);

        const eventAlbums = await getEventAlbums(event.id);

        const activeAlbums = eventAlbums
          .filter((album) => album.isActive)
          .sort(
            (a, b) =>
              a.displayOrder - b.displayOrder,
          );

        const results = await Promise.all(
          activeAlbums.map(async (album) => {
            const result = await getFavorites(
              eventSlug!,
              album.id,
            );

            return {
              albumId: album.id,
              items: result.items,
            };
          }),
        );

        if (cancelled) {
          return;
        }

        const allFavorites = results
          .flatMap(({ albumId, items }) =>
            items.map((item) => ({
              ...item,
              albumId,
            })),
          )
          .sort(
            (a, b) =>
              new Date(b.createdAt).getTime() -
              new Date(a.createdAt).getTime(),
          );

        setFavorites(allFavorites);
      } catch (err) {
        if (cancelled) {
          return;
        }

        setError(
          err instanceof Error
            ? err.message
            : "Não foi possível carregar os favoritos.",
        );
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadFavorites();

    return () => {
      cancelled = true;
    };
  }, [eventSlug]);

  const removeFavorite = async (
    favorite: FavoriteItem,
  ) => {
    if (!eventSlug) {
      return;
    }

    try {
      setFavoriteLoadingId(favorite.photoId);

      await unfavoritePhoto(
        eventSlug,
        favorite.albumId,
        favorite.photoId,
      );

      setFavorites((current) =>
        current.filter(
          (item) =>
            item.photoId !== favorite.photoId,
        ),
      );

      setSelectedPhotoIndex((currentIndex) => {
        if (currentIndex === null) {
          return null;
        }

        const removedIndex = favorites.findIndex(
          (item) =>
            item.photoId === favorite.photoId,
        );

        if (removedIndex === -1) {
          return currentIndex;
        }

        if (favorites.length === 1) {
          return null;
        }

        if (currentIndex > removedIndex) {
          return currentIndex - 1;
        }

        if (
          currentIndex === removedIndex &&
          currentIndex >= favorites.length - 1
        ) {
          return favorites.length - 2;
        }

        return currentIndex;
      });
    } catch (err) {
      console.error(
        "Erro ao remover favorito:",
        err,
      );
    } finally {
      setFavoriteLoadingId(null);
    }
  };

  /*
* ==========================================================
* Download todos os favoritos
* ==========================================================
*/


  const handleDownloadFavorites = async () => {
    if (!eventSlug || downloadingFavorites) {
      return;
    }

    setDownloadingFavorites(true);

    try {
      /*
       * Dá tempo para o React renderizar o spinner
       * antes do início do download.
       */
      await new Promise<void>((resolve) => {
        requestAnimationFrame(() => {
          requestAnimationFrame(() => {
            resolve();
          });
        });
      });

      const blob = await downloadFavorites(eventSlug);

      const url = window.URL.createObjectURL(blob);

      const link = document.createElement("a");

      link.href = url;
      link.download = "favoritos.zip";
      link.style.display = "none";

      document.body.appendChild(link);
      link.click();
      link.remove();

      window.setTimeout(() => {
        window.URL.revokeObjectURL(url);
      }, 1000);
    } catch (err) {
      console.error(
        "Erro ao baixar favoritos:",
        err,
      );
    } finally {
      window.setTimeout(() => {
        setDownloadingFavorites(false);
      }, 100);
    }
  };

  /*
* ==========================================================
* Download foto unica
* ==========================================================
*/

  const handleDownloadPhoto = async (photo: FavoriteItem) => {
    if (!eventSlug) {
      return;
    }

    try {
      const API_BASE_URL =
        import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5290";

      const response = await fetch(
        `${API_BASE_URL}/api/events/${encodeURIComponent(
          eventSlug,
        )}/albums/${photo.albumId}/photos/${photo.photoId}/download`,
        {
          credentials: "include",
        },
      );

      if (!response.ok) {
        throw new Error(
          `Não foi possível baixar a foto (${response.status}).`,
        );
      }

      const blob = await response.blob();

      const file = new File(
        [blob],
        photo.fileName,
        {
          type: blob.type || "image/jpeg",
        },
      );

      if (
        navigator.canShare &&
        navigator.canShare({ files: [file] })
      ) {
        await navigator.share({
          files: [file],
        });

        return;
      }

      const url = window.URL.createObjectURL(blob);

      const link = document.createElement("a");
      link.href = url;
      link.download = photo.fileName;

      document.body.appendChild(link);
      link.click();
      link.remove();

      window.URL.revokeObjectURL(url);
    } catch (error) {
      if (
        error instanceof DOMException &&
        error.name === "AbortError"
      ) {
        return;
      }

      console.error(
        "Erro ao compartilhar/baixar foto:",
        error,
      );
    }
  };

  /*
   * ==========================================================
   * FULLSCREEN
   * ==========================================================
   */

  const openPhoto = (index: number) => {
    setSelectedPhotoIndex(index);
  };

  const closeViewer = () => {
    setSelectedPhotoIndex(null);
  };

  const showPreviousPhoto = () => {
    setSelectedPhotoIndex((currentIndex) => {
      if (currentIndex === null || currentIndex <= 0) {
        return currentIndex;
      }

      return currentIndex - 1;
    });
  };

  const showNextPhoto = () => {
    setSelectedPhotoIndex((currentIndex) => {
      if (
        currentIndex === null ||
        currentIndex >= favorites.length - 1
      ) {
        return currentIndex;
      }

      return currentIndex + 1;
    });
  };

  const selectedPhoto =
    selectedPhotoIndex !== null
      ? favorites[selectedPhotoIndex]
      : null;

  const goHome = () => {
    if (!eventSlug) {
      return;
    }

    navigate(`/${eventSlug}`);
  };

  const goBack = () => {
    if (!eventSlug) {
      return;
    }

    navigate(`/${eventSlug}`);
  };

  return (
    <main className="favorites">
      {/* ======================================================
          FOTO DE CAPA
          ====================================================== */}

      <div className="favorites__hero">
        <img
          src={fotoTopo}
          alt=""
          className="favorites__hero-image"
        />
      </div>

      {/* ======================================================
          FUNDO BRANCO
          ====================================================== */}

      <img
        src={fundoBranco}
        alt=""
        className="favorites__background"
      />

      {/* ======================================================
          BOTÃO INÍCIO
          ====================================================== */}

      <button
        type="button"
        className="favorites__home"
        onClick={goHome}
        aria-label="Voltar para início"
      >
        <img
          src={botaoInicio}
          alt=""
        />
      </button>

      {/* ======================================================
          BOTÃO VOLTAR
          ====================================================== */}

      <button
        type="button"
        className="favorites__back"
        onClick={goBack}
        aria-label="Voltar"
      >
        <img
          src={voltar}
          alt=""
        />
      </button>

      {/* ======================================================
          TÍTULO
          ====================================================== */}

      <img
        src={tituloFavoritos}
        alt="Favoritos"
        className="favorites__title"
      />

      {/* ======================================================
          CARREGANDO
          ====================================================== */}

      {loading && (
        <div className="favorites__state favorites__state--loading">
          Carregando favoritos...
        </div>
      )}

      {/* ======================================================
          ERRO
          ====================================================== */}

      {!loading && error && (
        <div className="favorites__state favorites__state--error">
          {error}
        </div>
      )}

      {/* ======================================================
          VAZIO
          ====================================================== */}

      {!loading &&
        !error &&
        favorites.length === 0 && (
          <div className="favorites__empty">
            Você ainda não adicionou nenhuma
            foto aos favoritos.
          </div>
        )}

      {/* ======================================================
          TÍTULO FOTOS
          ====================================================== */}

      <img
        className="gallery__title"
        src={tituloFotos}
        alt="Fotos"
      />

      {/* ======================================================
          GALERIA
          ====================================================== */}

      {!loading &&
        !error &&
        favorites.length > 0 && (
          <section className="favorites__grid">
            {favorites.map((photo, index) => (
              <div
                className="favorites__photo-wrapper"
                key={photo.photoId}
              >
                {/* FOTO */}

                <button
                  type="button"
                  className="favorites__photo"
                  onClick={() => openPhoto(index)}
                  aria-label={`Abrir ${photo.fileName}`}
                >
                  <img
                    src={photo.thumbnailUrl}
                    alt={photo.fileName}
                    loading="lazy"
                  />
                </button>

                {/* FAVORITO */}

                <button
                  type="button"
                  className={`favorites__favorite ${favoriteLoadingId === photo.photoId
                    ? "favorites__favorite--loading"
                    : "favorites__favorite--active"
                    }`}
                  onClick={(event) => {
                    event.stopPropagation();

                    void removeFavorite(photo);
                  }}
                  disabled={
                    favoriteLoadingId === photo.photoId
                  }
                  aria-label="Remover dos favoritos"
                >
                  <img
                    src={favoritoIcon}
                    alt=""
                    className="favorites__favorite-icon"
                  />
                </button>
              </div>
            ))}
          </section>
        )}

      {/* ======================================================
          DOWNLOAD DOS FAVORITOS
          ====================================================== */}

      {!loading &&
        !error &&
        favorites.length > 0 && (
          <button
            type="button"
            className="favorites__download"
            onClick={() => {
              void handleDownloadFavorites();
            }}
            disabled={downloadingFavorites}
            aria-label={
              downloadingFavorites
                ? "Baixando favoritos"
                : "Baixar favoritos"
            }
          >
            {downloadingFavorites ? (
              <span
                className="favorites__download-spinner"
                aria-hidden="true"
              />
            ) : (
              <div className="favorites__download-content">
                <img
                  src={iconeDownload}
                  alt=""
                  className="favorites__download-icon"
                />

                <div className="favorites__download-text">
                  <img
                    src={textoDownload}
                    alt="Download"
                    className="favorites__download-title"
                  />

                  <img
                    src={textoBaixeFotos}
                    alt="Baixe todas as fotos deste álbum."
                    className="favorites__download-description"
                  />
                </div>
              </div>
            )}
          </button>
        )}

      {/* ======================================================
          FULLSCREEN
          ====================================================== */}

      {selectedPhoto && (
        <div
          className="favorites__viewer"
          role="dialog"
          aria-modal="true"
          aria-label="Visualizador de foto"
          onClick={closeViewer}
        >
          {/* DOWNLOAD */}
          <div className="favorites__viewer-actions">
            <button
              type="button"
              className="favorites__download-photo"
              onClick={(event) => {
                event.stopPropagation();

                void handleDownloadPhoto(selectedPhoto);
              }}
              aria-label={`Baixar ${selectedPhoto.fileName}`}
            >
              <img
                src={iconeDownload}
                alt=""
              />
            </button>

            { /* FECHAR */ }
            <button
              type="button"
              className="favorites__viewer-close"
              onClick={closeViewer}
              aria-label="Fechar foto"
            >
              ×
            </button>
          </div>



          {/* ANTERIOR */}

          {selectedPhotoIndex !== null &&
            selectedPhotoIndex > 0 && (
              <button
                type="button"
                className="favorites__viewer-prev"
                onClick={(event) => {
                  event.stopPropagation();
                  showPreviousPhoto();
                }}
                aria-label="Foto anterior"
              >
                ‹
              </button>
            )}

          {/* FOTO */}

          <img
            src={selectedPhoto.displayUrl}
            alt={selectedPhoto.fileName}
            className="favorites__viewer-image"
            onClick={(event) => {
              event.stopPropagation();
            }}
          />

          {/* PRÓXIMA */}

          {selectedPhotoIndex !== null &&
            selectedPhotoIndex <
            favorites.length - 1 && (
              <button
                type="button"
                className="favorites__viewer-next"
                onClick={(event) => {
                  event.stopPropagation();
                  showNextPhoto();
                }}
                aria-label="Próxima foto"
              >
                ›
              </button>
            )}
        </div>
      )}
    </main>
  );
}

export default Favorites;