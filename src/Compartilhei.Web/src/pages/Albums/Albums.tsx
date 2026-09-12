import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

import "./Albums.css";

import fotoTopo from "../../assets/images/whats-app-x-0020-image-x-0020-2026-08-30-x-0020-at-x-0020-17-31-08-jpeg0.png";
import fundoBranco from "../../assets/decorations/fundo-branco0.svg";
import tituloAlbuns from "../../assets/logo/titulo-alb-ns0.svg";

import recepcaoIcon from "../../assets/icons/vector2.svg";
import recepcaoIconInside from "../../assets/icons/group3.svg";
import recepcaoNome from "../../assets/icons/texto-12.svg";
import recepcaoDescricao from "../../assets/icons/descri-o2.svg";

import cerimoniaIcon from "../../assets/icons/cerim-nia0.svg";

import festaIcon from "../../assets/icons/vector0.svg";
import festaIconInside from "../../assets/icons/group1.svg";
import festaNome from "../../assets/icons/texto-10.svg";
import festaDescricao from "../../assets/icons/descri-o0.svg";

import afterPartyIcon from "../../assets/icons/vector1.svg";
import afterPartyIconInside from "../../assets/icons/group2.svg";
import afterPartyNome from "../../assets/icons/texto-11.svg";
import afterPartyDescricao from "../../assets/icons/descri-o1.svg";

import botaoInicio from "../../assets/icons/bot-o-de-inicio0.svg";

import {
  getEventAlbums,
  getEventBySlug,
  type AlbumResponse,
} from "../../services/eventsApi";

export default function Albums() {
  const navigate = useNavigate();
  const { eventSlug } = useParams();

  const [albums, setAlbums] = useState<AlbumResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!eventSlug) {
      setError("Evento não informado.");
      setLoading(false);
      return;
    }

    let cancelled = false;

    async function loadAlbums() {
      if (!eventSlug) {
        return;
      }
      try {
        setLoading(true);
        setError(null);

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
      } catch (err) {
        if (cancelled) {
          return;
        }

        setError(
          err instanceof Error
            ? err.message
            : "Não foi possível carregar os álbuns.",
        );
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadAlbums();

    return () => {
      cancelled = true;
    };
  }, [eventSlug]);

  const goHome = () => {
    if (!eventSlug) {
      return;
    }

    navigate(`/${eventSlug}`);
  };

  const openAlbum = (album: AlbumResponse) => {
    if (!eventSlug) {
      return;
    }

    navigate(
      `/${eventSlug}/albuns/${album.id}`,
    );
  };

  const getAlbumByName = (name: string) =>
    albums.find(
      (album) =>
        album.name.toLowerCase() ===
        name.toLowerCase(),
    );

  const recepcao = getAlbumByName("Recepção");
  const cerimonia = getAlbumByName("Cerimônia");
  const festa = getAlbumByName("Festa");
  const afterParty = getAlbumByName("After Party");

  return (
    <main className="albums">
      <img
        className="albums__top-image"
        src={fotoTopo}
        alt=""
      />

      <img
        className="albums__white-background"
        src={fundoBranco}
        alt=""
      />

      <img
        className="albums__title"
        src={tituloAlbuns}
        alt="Álbuns"
      />

      {loading && (
        <div className="albums__loading">
          Carregando álbuns...
        </div>
      )}

      {error && (
        <div className="albums__error">
          {error}
        </div>
      )}

      {!loading && !error && (
        <section className="albums__list">
          {/* RECEPÇÃO */}
          <button
            type="button"
            className="album-item album-item--recepcao"
            onClick={() => {
              if (recepcao) {
                openAlbum(recepcao);
              }
            }}
            disabled={!recepcao}
          >
            <span className="album-item__icon">
              <img
                src={recepcaoIcon}
                alt=""
              />
              <img
                src={recepcaoIconInside}
                alt=""
              />
            </span>

            <img
              className="album-item__name album-item__name--recepcao"
              src={recepcaoNome}
              alt="Recepção"
            />

            <img
              className="album-item__description album-item__description--recepcao"
              src={recepcaoDescricao}
              alt=""
            />
          </button>

          {/* CERIMÔNIA */}
          <button
            type="button"
            className="album-item album-item--cerimonia"
            onClick={() => {
              if (cerimonia) {
                openAlbum(cerimonia);
              }
            }}
            disabled={!cerimonia}
          >
            <img
              className="album-item__cerimonia"
              src={cerimoniaIcon}
              alt="Cerimônia"
            />
          </button>

          {/* FESTA */}
          <button
            type="button"
            className="album-item album-item--festa"
            onClick={() => {
              if (festa) {
                openAlbum(festa);
              }
            }}
            disabled={!festa}
          >
            <span className="album-item__icon">
              <img
                src={festaIcon}
                alt=""
              />
              <img
                src={festaIconInside}
                alt=""
              />
            </span>

            <img
              className="album-item__name album-item__name--festa"
              src={festaNome}
              alt="Festa"
            />

            <img
              className="album-item__description album-item__description--festa"
              src={festaDescricao}
              alt=""
            />
          </button>

          {/* AFTER PARTY */}
          <button
            type="button"
            className="album-item album-item--after-party"
            onClick={() => {
              if (afterParty) {
                openAlbum(afterParty);
              }
            }}
            disabled={!afterParty}
          >
            <span className="album-item__icon">
              <img
                src={afterPartyIcon}
                alt=""
              />
              <img
                src={afterPartyIconInside}
                alt=""
              />
            </span>

            <img
              className="album-item__name album-item__name--after-party"
              src={afterPartyNome}
              alt="After Party"
            />

            <img
              className="album-item__description album-item__description--after-party"
              src={afterPartyDescricao}
              alt=""
            />
          </button>
        </section>
      )}

      <button
        type="button"
        className="albums__home-button"
        onClick={goHome}
        aria-label="Voltar para início"
      >
        <img
          src={botaoInicio}
          alt=""
        />
      </button>
    </main>
  );
}