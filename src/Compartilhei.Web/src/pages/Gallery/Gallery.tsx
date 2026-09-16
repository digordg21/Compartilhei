import { useCallback, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

import { PhotoUploadFlow } from "../../components/photos/PhotoUploadFlow/PhotoUploadFlow";

import "./Gallery.css";

import {
  getPhotoGallery,
  type PhotoGalleryItem,
} from "../../services/galleryApi";

import {
  favoritePhoto,
  unfavoritePhoto,
} from "../../services/favoritesApi";


/*
 * ============================================================
 * ASSETS GERAIS
 * ============================================================
 */

// MESMA FOTO DE TOPO UTILIZADA NA TELA ALBUMS.
// Essa é a foto da Luna que você já está usando.
import fotoTopo from "../../assets/images/whats-app-x-0020-image-x-0020-2026-08-30-x-0020-at-x-0020-17-31-08-jpeg0.png";

// MESMO FUNDO BRANCO DA TELA ALBUMS.
import fundoBranco from "../../assets/decorations/fundo-branco0.svg";

// MESMO BOTÃO DE INÍCIO DA TELA ALBUMS.
import botaoInicio from "../../assets/icons/bot-o-de-inicio0.svg";

// BOTÃO VOLTAR DA GALLERY.
import voltar from "../../assets/festa/voltar0.svg";

// TEXTO "FOTOS".
import tituloFotos from "../../assets/festa/texto-fotos0.svg";

// botao favorito
import favoritoIcon from "../../assets/decorations/BOTÃO CORAÇÃO vermelho svg.svg";


/*
 * ============================================================
 * CAPAS DOS ÁLBUNS
 * ============================================================
 *
 * IMPORTANTE:
 * Os caminhos abaixo são PLACEHOLDERS.
 *
 * Substitua pelo nome/caminho real das imagens de cada álbum.
 *
 * A ideia é NÃO colocar isso no backend.
 * Para o nosso MVP, deixaremos as 4 capas hardcoded aqui.
 */

// FOTO DE CAPA DA RECEPÇÃO
// TODO: substituir pelo arquivo real.
import capaRecepcao from "../../assets/images/FOTO RECEPÇÃO.jpg";

// FOTO DE CAPA DA CERIMÔNIA
// TODO: substituir pelo arquivo real.
import capaCerimonia from "../../assets/images/FOTO CERIMONIA.jpg";

// FOTO DE CAPA DA FESTA
// TODO: substituir pelo arquivo real.
import capaFesta from "../../assets/images/FOTO FESTA.jpg";

// FOTO DE CAPA DO AFTER PARTY
// TODO: substituir pelo arquivo real.
import capaAfterParty from "../../assets/images/FOTO AFTER PARTY.jpg";


/*
 * ============================================================
 * TÍTULOS DOS ÁLBUNS
 * ============================================================
 *
 * Aqui também deixamos preparado para cada álbum possuir
 * seu próprio asset do Figma.
 *
 * No momento, o asset da Festa já existe.
 */

// TÍTULO RECEPÇÃO
// TODO: substituir pelo SVG real do Figma.
import tituloRecepcao from "../../assets/decorations/TEXTO RECEPÇÃO.svg";

// TÍTULO CERIMÔNIA
// TODO: substituir pelo SVG real do Figma.
import tituloCerimonia from "../../assets/decorations/TEXTO CERIMONIA.svg";

// TÍTULO FESTA
import tituloFesta from "../../assets/festa/titulo-festa0.svg";

// TÍTULO AFTER PARTY
// TODO: substituir pelo SVG real do Figma.
import tituloAfterParty from "../../assets/decorations/TEXTO AFTER PARTY.svg";


/*
 * ============================================================
 * CONFIGURAÇÃO DOS 4 ÁLBUNS
 * ============================================================
 *
 * O componente Gallery continua sendo UM SÓ.
 *
 * O albumId da URL determina qual configuração será utilizada.
 *
 * Hoje temos o ID real da Festa.
 * Os demais IDs precisam ser substituídos pelos IDs reais
 * dos seus respectivos álbuns.
 */

type GalleryAlbumConfig = {
  cover: string;
  albumTitle: string;
};


const albumConfig: Record<string, GalleryAlbumConfig> = {
  "d29b21ec-2cb3-4c87-8f9f-47436a221fce": {
    cover: capaRecepcao,
    albumTitle: tituloRecepcao,
  },

  "c39743dd-5acc-465a-8552-f92b576d9aec": {
    cover: capaCerimonia,
    albumTitle: tituloCerimonia,
  },

  "4300237f-938c-4451-a04c-8a25a4bbe268": {
    cover: capaFesta,
    albumTitle: tituloFesta,
  },

  "b715cde1-de9e-4f57-8f60-5bf0050847ff": {
    cover: capaAfterParty,
    albumTitle: tituloAfterParty,
  },
};


/*
 * Configuração usada quando o albumId não estiver cadastrado.
 *
 * Isso impede a tela de quebrar caso um álbum novo apareça.
 */
const defaultAlbumConfig: GalleryAlbumConfig = {
  cover: fotoTopo,
  albumTitle: tituloFotos,
};


export default function Gallery() {

  const navigate = useNavigate();

  const { eventSlug, albumId } = useParams();


  /*
   * ==========================================================
   * CONFIGURAÇÃO DO ÁLBUM ATUAL
   * ==========================================================
   */

  const currentAlbum =
    albumId
      ? albumConfig[albumId.toLowerCase()]
      ?? defaultAlbumConfig
      : defaultAlbumConfig;


  /*
   * ==========================================================
   * ESTADOS
   * ==========================================================
   */

  const [photos, setPhotos] =
    useState<PhotoGalleryItem[]>([]);

  const [loading, setLoading] =
    useState(true);

  const [error, setError] =
    useState<string | null>(null);

  const [nextCursor, setNextCursor] =
    useState<string | null>(null);

  const [hasMore, setHasMore] =
    useState(false);

  const [loadingMore, setLoadingMore] =
    useState(false);

  const [selectedPhoto, setSelectedPhoto] =
    useState<PhotoGalleryItem | null>(null);

  const [favoriteLoadingId, setFavoriteLoadingId] =
    useState<string | null>(null);

  const [isPhotoUploadOpen, setIsPhotoUploadOpen] =
    useState(false);


/*
 * ==========================================================
 * CARREGAMENTO DA GALERIA
 * ==========================================================
 */

const loadGallery = useCallback(async () => {
  if (!eventSlug || !albumId) {
    setError("Álbum não informado.");
    setLoading(false);
    return;
  }

  try {
    setLoading(true);
    setError(null);

    const result = await getPhotoGallery(
      eventSlug,
      albumId,
    );

    setPhotos(result.items);
    setNextCursor(result.nextCursor);
    setHasMore(result.hasMore);
  } catch (err) {
    setError(
      err instanceof Error
        ? err.message
        : "Não foi possível carregar a galeria.",
    );
  } finally {
    setLoading(false);
  }
}, [eventSlug, albumId]);

useEffect(() => {
  void loadGallery();
}, [loadGallery]);


  /*
   * ==========================================================
   * PAGINAÇÃO
   * ==========================================================
   */

  const loadMorePhotos = async () => {

    if (
      !eventSlug ||
      !albumId ||
      !nextCursor ||
      !hasMore ||
      loadingMore
    ) {
      return;
    }


    try {

      setLoadingMore(true);


      const result =
        await getPhotoGallery(
          eventSlug!,
          albumId!,
          nextCursor,
        );


      setPhotos((current) => [
        ...current,
        ...result.items,
      ]);


      setNextCursor(result.nextCursor);

      setHasMore(result.hasMore);

    } catch (err) {

      console.error(
        "Erro ao carregar mais fotos:",
        err,
      );

    } finally {

      setLoadingMore(false);

    }

  };


  /*
   * ==========================================================
   * NAVEGAÇÃO
   * ==========================================================
   */

  const goBack = () => {

    if (!eventSlug) {
      return;
    }


    navigate(`/${eventSlug}/albuns`);

  };


  const goHome = () => {

    if (!eventSlug) {
      return;
    }


    navigate(`/${eventSlug}`);

  };


  /*
   * ==========================================================
   * FAVORITO
   * ==========================================================
   */

  const toggleFavorite = async (
    photo: PhotoGalleryItem,
  ) => {

    if (
      !eventSlug ||
      !albumId ||
      favoriteLoadingId === photo.id
    ) {
      return;
    }


    try {

      setFavoriteLoadingId(photo.id);


      const result =
        photo.isFavorite

          ? await unfavoritePhoto(
            eventSlug,
            albumId,
            photo.id,
          )

          : await favoritePhoto(
            eventSlug,
            albumId,
            photo.id,
          );


      setPhotos((current) =>
        current.map((item) =>
          item.id === photo.id
            ? {
              ...item,
              isFavorite:
                result.isFavorited,
            }
            : item,
        ),
      );

    } catch (err) {

      console.error(
        "Erro ao alterar favorito:",
        err,
      );

    } finally {

      setFavoriteLoadingId(null);

    }

  };


  /*
   * ==========================================================
   * FULLSCREEN
   * ==========================================================
   */

  const openPhoto = (
    photo: PhotoGalleryItem,
  ) => {

    setSelectedPhoto(photo);

  };


  const closeViewer = () => {

    setSelectedPhoto(null);

  };

  const downloadPhoto = async (photo: PhotoGalleryItem) => {
    if (!eventSlug || !albumId) {
      return;
    }

    try {
      const API_BASE_URL =
        import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5290";

      const response = await fetch(
        `${API_BASE_URL}/api/events/${encodeURIComponent(
          eventSlug,
        )}/albums/${albumId}/photos/${photo.id}/download`,
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

      // Fallback para navegadores que não suportam compartilhamento de arquivos.
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

      console.error("Erro ao compartilhar/baixar foto:", error);
    }
  };


  /*
   * ==========================================================
   * LOADING
   * ==========================================================
   */

  if (loading) {

    return (

      <main className="gallery">

        <section className="gallery__hero">

          <img
            className="gallery__hero-image"
            src={currentAlbum.cover}
            alt=""
          />

        </section>


        <img
          className="gallery__background"
          src={fundoBranco}
          alt=""
          aria-hidden="true"
        />


        <div className="gallery__state">
          Carregando fotos...
        </div>

      </main>

    );

  }


  /*
   * ==========================================================
   * ERRO
   * ==========================================================
   */

  if (error) {

    return (

      <main className="gallery">

        <section className="gallery__hero">

          <img
            className="gallery__hero-image"
            src={currentAlbum.cover}
            alt=""
          />

        </section>


        <img
          className="gallery__background"
          src={fundoBranco}
          alt=""
          aria-hidden="true"
        />


        <button
          type="button"
          className="gallery__home"
          onClick={goHome}
          aria-label="Voltar para início"
        >
          <img
            src={botaoInicio}
            alt=""
          />
        </button>


        <button
          type="button"
          className="gallery__back"
          onClick={goBack}
          aria-label="Voltar para álbuns"
        >
          <img
            src={voltar}
            alt=""
          />
        </button>


        <img
          className="gallery__album-title"
          src={currentAlbum.albumTitle}
          alt=""
        />


        <img
          className="gallery__title"
          src={tituloFotos}
          alt="Fotos"
        />


        <div className="gallery__state gallery__state--error">
          {error}
        </div>

      </main>

    );

  }


  /*
   * ==========================================================
   * TELA PRINCIPAL
   * ==========================================================
   */

  return (

    <main className="gallery">


      {/* ======================================================
          FOTO DE CAPA DO ÁLBUM
          ====================================================== */}

      <section className="gallery__hero">

        <img
          className="gallery__hero-image"
          src={currentAlbum.cover}
          alt=""
        />


        {/* ====================================================
            NOME DO ÁLBUM
            ====================================================

            O asset muda conforme o albumId.

            Festa:
            tituloFesta

            Recepção:
            tituloRecepcao

            Cerimônia:
            tituloCerimonia

            After Party:
            tituloAfterParty
        */}

        <img
          className="gallery__album-title"
          src={currentAlbum.albumTitle}
          alt=""
        />

      </section>


      {/* ======================================================
          FUNDO BRANCO
          ====================================================== */}

      <img
        className="gallery__background"
        src={fundoBranco}
        alt=""
        aria-hidden="true"
      />


      {/* ======================================================
          BOTÃO HOME
          ====================================================== */}

      <button
        type="button"
        className="gallery__home"
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
        className="gallery__back"
        onClick={goBack}
        aria-label="Voltar para álbuns"
      >

        <img
          src={voltar}
          alt=""
        />

      </button>

      <button
        type="button"
        className="gallery__camera"
        onClick={() => setIsPhotoUploadOpen(true)}
        aria-label="Adicionar fotos"
      >
        📷
      </button>


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

      {photos.length === 0 ? (

        <div className="gallery__empty">

          Ainda não existem fotos neste álbum.

        </div>

      ) : (

        <section className="gallery__grid">

          {photos.map((photo) => (

            <div
              className="gallery__photo-wrapper"
              key={photo.id}
            >

              {/* FOTO */}

              <button
                type="button"
                className="gallery__photo"
                onClick={() =>
                  openPhoto(photo)
                }
                aria-label={
                  `Abrir ${photo.fileName}`
                }
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
                className={`gallery__favorite ${photo.isFavorite
                    ? "gallery__favorite--active"
                    : ""
                  }`}
                onClick={(event) => {
                  event.stopPropagation();
                  void toggleFavorite(photo);
                }}
                disabled={favoriteLoadingId === photo.id}
                aria-label={
                  photo.isFavorite
                    ? "Remover dos favoritos"
                    : "Adicionar aos favoritos"
                }
              >
                <img
                  src={favoritoIcon}
                  alt=""
                  className="gallery__favorite-icon"
                />
              </button>

            </div>

          ))}

        </section>

      )}


      {/* ======================================================
          PAGINAÇÃO
          ====================================================== */}

      {hasMore && (

        <div
          className="gallery__load-more"
          ref={(element) => {

            if (!element) {
              return;
            }


            const observer =
              new IntersectionObserver(
                (entries) => {

                  if (
                    entries[0]?.isIntersecting
                  ) {

                    void loadMorePhotos();

                  }

                },
                {
                  rootMargin: "300px",
                },
              );


            observer.observe(element);


            return () => {

              observer.disconnect();

            };

          }}
        >

          {loadingMore &&
            "Carregando mais fotos..."}

        </div>

      )}


      {/* ======================================================
          FULLSCREEN
          ====================================================== */}

      {selectedPhoto && (

        <div
          className="gallery__viewer"
          role="dialog"
          aria-modal="true"
          aria-label="Visualizador de foto"
          onClick={closeViewer}
        >

          <button
            type="button"
            className="gallery__viewer-close"
            onClick={closeViewer}
            aria-label="Fechar foto"
          >
            ×
          </button>

          <button
            type="button"
            className="gallery__viewer-download"
            onClick={(event) => {
              event.stopPropagation();
              void downloadPhoto(selectedPhoto);
            }}
            aria-label="Baixar foto"
          >
            ↓
          </button>


          <img
            src={selectedPhoto.displayUrl}
            alt={selectedPhoto.fileName}
            className="gallery__viewer-image"
            onClick={(event) =>
              event.stopPropagation()
            }
          />

        </div>

      )}
      {eventSlug && albumId && (
        <PhotoUploadFlow
          eventSlug={eventSlug}
          albumId={albumId}
          open={isPhotoUploadOpen}
          onClose={() => setIsPhotoUploadOpen(false)}
          onSuccess={() => {
            setIsPhotoUploadOpen(false);
            void loadGallery();
          }}
        />
      )}
    </main>

  );
}