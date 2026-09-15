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
  type FavoritePhotoItem,
} from "../../services/favoritesApi";

import fotoTopo from "../../assets/images/whats-app-x-0020-image-x-0020-2026-08-30-x-0020-at-x-0020-17-31-08-jpeg0.png";
import fundoBranco from "../../assets/decorations/fundo-branco0.svg";
import botaoInicio from "../../assets/icons/bot-o-de-inicio0.svg";
import tituloFavoritos from "../../assets/decorations/TEXTO FAVORITOS.svg";
import voltar from "../../assets/festa/voltar0.svg";
import tituloFotos from "../../assets/festa/texto-fotos0.svg";

// Mesmo ícone de favorito utilizado no Figma/Welcome.
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
    } catch (err) {
      console.error(
        "Erro ao remover favorito:",
        err,
      );
    } finally {
      setFavoriteLoadingId(null);
    }
  };

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

            {favorites.map((photo) => (
              <div
                className="favorites__photo-wrapper"
                key={photo.photoId}
              >

                {/* FOTO */}

                <button
                  type="button"
                  className="favorites__photo"
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
                  className={`favorites__favorite ${
                    favoriteLoadingId === photo.photoId
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
    </main>
  );
}

export default Favorites;