import "./Welcome.css";

import lunaWedding
    from "../../assets/welcome/images/luna-wedding0.png";

import tituloWedding
    from "../../assets/welcome/decorations/titulo-wedding0.svg";

import fundoBranco
    from "../../assets/welcome/decorations/fundo-branco0.svg";

import textoLuna
    from "../../assets/welcome/decorations/texto-luna0.svg";

import vectorCamera
    from "../../assets/welcome/icons/vector0.svg";

import cameraIcon
    from "../../assets/welcome/icons/group2.svg";

import cameraText
    from "../../assets/welcome/icons/texto-12.svg";

import vectorFavorite
    from "../../assets/welcome/icons/vector1.svg";

import favoriteIcon
    from "../../assets/welcome/icons/group3.svg";

import favoriteText
    from "../../assets/welcome/icons/texto-13.svg";

import albumsIcon
    from "../../assets/welcome/icons/group1.svg";

import albumsText
    from "../../assets/welcome/icons/texto-11.svg";

import downloadIcon
    from "../../assets/welcome/icons/group0.svg";

import downloadText
    from "../../assets/welcome/icons/texto-10.svg";

import { useNavigate, useParams } from "react-router-dom";
import { useState } from "react";

import { PhotoUploadFlow } from "../../components/photos/PhotoUploadFlow/PhotoUploadFlow";

function Welcome() {
    const navigate = useNavigate();

    const { eventSlug } = useParams<{
        eventSlug: string;
    }>();

    const [isPhotoUploadOpen, setIsPhotoUploadOpen] =
        useState(false);

    return (
        <main className="welcome">
            <section className="welcome__hero">
                <img
                    className="welcome__hero-image"
                    src={lunaWedding}
                    alt="Luna"
                />

                <img
                    className="welcome__hero-title"
                    src={tituloWedding}
                    alt="Wedding"
                />
            </section>

            <section className="welcome__content">
                <img
                    className="welcome__background"
                    src={fundoBranco}
                    alt=""
                    aria-hidden="true"
                />

                <img
                    className="welcome__texto-luna"
                    src={textoLuna}
                    alt=""
                    aria-hidden="true"
                />

                <button
                    className="welcome__action welcome__action--camera"
                    onClick={() => setIsPhotoUploadOpen(true)}
                >
                    <span className="welcome__action-icon">
                        <img
                            className="welcome__action-base"
                            src={vectorCamera}
                            alt=""
                            aria-hidden="true"
                        />

                        <img
                            className="welcome__action-symbol"
                            src={cameraIcon}
                            alt=""
                            aria-hidden="true"
                        />
                    </span>

                    <img
                        className="welcome__action-label"
                        src={cameraText}
                        alt="Câmera"
                    />
                </button>

                <button
                    className="welcome__action welcome__action--favorite"
                    onClick={() =>
                        navigate(`/${eventSlug}/favoritos`)
                    }
                >
                    <span className="welcome__action-icon">
                        <img
                            className="welcome__action-base"
                            src={vectorFavorite}
                            alt=""
                            aria-hidden="true"
                        />

                        <img
                            className="welcome__action-symbol"
                            src={favoriteIcon}
                            alt=""
                            aria-hidden="true"
                        />
                    </span>

                    <img
                        className="welcome__action-label"
                        src={favoriteText}
                        alt="Favoritos"
                    />
                </button>

                <button
                    className="welcome__action welcome__action--albums"
                    onClick={() =>
                        navigate(`/${eventSlug}/albuns`)
                    }
                >
                    <span className="welcome__action-icon">
                        <img
                            className="welcome__action-base"
                            src={vectorFavorite}
                            alt=""
                            aria-hidden="true"
                        />

                        <img
                            className="welcome__action-symbol"
                            src={albumsIcon}
                            alt=""
                            aria-hidden="true"
                        />
                    </span>

                    <img
                        className="welcome__action-label"
                        src={albumsText}
                        alt="Álbuns"
                    />
                </button>

                <button
                    className="welcome__action welcome__action--download"
                    onClick={() =>
                        navigate(`/${eventSlug}/download`)
                    }
                >
                    <span className="welcome__action-icon">
                        <img
                            className="welcome__action-base"
                            src={vectorCamera}
                            alt=""
                            aria-hidden="true"
                        />

                        <img
                            className="welcome__action-symbol"
                            src={downloadIcon}
                            alt=""
                            aria-hidden="true"
                        />
                    </span>

                    <img
                        className="welcome__action-label"
                        src={downloadText}
                        alt="Download"
                    />
                </button>
            </section>

            {eventSlug && (
                <PhotoUploadFlow
                    eventSlug={eventSlug}
                    open={isPhotoUploadOpen}
                    onClose={() =>
                        setIsPhotoUploadOpen(false)
                    }
                />
            )}
        </main>
    );
}

export default Welcome;