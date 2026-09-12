import './Welcome.css';

import lunaWedding
    from '../../assets/welcome/images/luna-wedding0.png';

import tituloWedding
    from '../../assets/welcome/decorations/titulo-wedding0.svg';

import fundoBranco
    from '../../assets/welcome/decorations/fundo-branco0.svg';

import textoLuna
    from '../../assets/welcome/decorations/texto-luna0.svg';

import vectorCamera
    from '../../assets/welcome/icons/vector0.svg';

import cameraIcon
    from '../../assets/welcome/icons/group2.svg';

import cameraText
    from '../../assets/welcome/icons/texto-12.svg';

import vectorFavorite
    from '../../assets/welcome/icons/vector1.svg';

import favoriteIcon
    from '../../assets/welcome/icons/group3.svg';

import favoriteText
    from '../../assets/welcome/icons/texto-13.svg';

import albumsIcon
    from '../../assets/welcome/icons/group1.svg';

import albumsText
    from '../../assets/welcome/icons/texto-11.svg';

import downloadIcon
    from '../../assets/welcome/icons/group0.svg';

import downloadText
    from '../../assets/welcome/icons/texto-10.svg';

import { useNavigate, useParams } from 'react-router-dom';
import { useRef, useState } from "react";
import { PhotoSourceModal } from "../../components/photos/PhotoSourceModal/PhotoSourceModal";
import { usePhotoUpload } from "../../hooks/usePhotoUpload";
import { PhotoUploadQueue } from "../../components/photos/PhotoUploadQueue/PhotoUploadQueue";
import { requestPhotoUpload, uploadPhotoToBlob, confirmPhotoUpload } from "../../services/photosApi";


function Welcome() {
    const navigate = useNavigate();
    const { eventSlug } = useParams<{ eventSlug: string }>();

    const [isPhotoSourceOpen, setIsPhotoSourceOpen] = useState(false);

    const cameraInputRef = useRef<HTMLInputElement>(null);
    const galleryInputRef = useRef<HTMLInputElement>(null);

    const openCamera = () => {
        setIsPhotoSourceOpen(false);
        cameraInputRef.current?.click();
    };

    const openGallery = () => {
        setIsPhotoSourceOpen(false);
        galleryInputRef.current?.click();
    };

    const {
        photos,
        addFiles,
        updatePhoto,
        removeFile,
    } = usePhotoUpload();

    const [isPhotoQueueOpen, setIsPhotoQueueOpen] = useState(false);

    const handleUpload = async () => {
        if (!eventSlug) {
            return;
        }

        // Para o primeiro teste, vamos usar um álbum real
        // posteriormente substituído pelo AlbumId selecionado.
        const albumId = "4300237f-938c-4451-a04c-8a25a4bbe268";

        for (const photo of photos) {
            try {
                updatePhoto(photo.id, {
                    status: "Uploading",
                    progress: 0,
                    error: undefined,
                });

                const authorization =
                    await requestPhotoUpload(
                        eventSlug,
                        albumId,
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

                const confirmation =
                    await confirmPhotoUpload(
                        eventSlug,
                        albumId,
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
        }
    };

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
                    onClick={() => setIsPhotoSourceOpen(true)}
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
                    onClick={() => navigate(`/${eventSlug}/favoritos`)}
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
                    onClick={() => navigate(`/${eventSlug}/albuns`)}
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
                    onClick={() => navigate(`/${eventSlug}/download`)}
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

                <input
                    ref={cameraInputRef}
                    type="file"
                    accept="image/jpeg,image/png,image/webp"
                    capture="environment"
                    hidden
                />

                <input
                    ref={galleryInputRef}
                    type="file"
                    accept="image/jpeg,image/png,image/webp"
                    multiple
                    hidden
                />

                <input
                    ref={cameraInputRef}
                    type="file"
                    accept="image/jpeg,image/png,image/webp"
                    capture="environment"
                    hidden
                    onChange={(event) => {
                        if (event.target.files?.length) {
                            addFiles(event.target.files);
                            setIsPhotoQueueOpen(true);
                        }

                        event.target.value = "";
                    }}
                />

                <input
                    ref={galleryInputRef}
                    type="file"
                    accept="image/jpeg,image/png,image/webp"
                    multiple
                    hidden
                    onChange={(event) => {
                        if (event.target.files?.length) {
                            addFiles(event.target.files);
                            setIsPhotoQueueOpen(true);
                        }

                        event.target.value = "";
                    }}
                />
            </section>
            <PhotoSourceModal
                open={isPhotoSourceOpen}
                onClose={() => setIsPhotoSourceOpen(false)}
                onCamera={openCamera}
                onGallery={openGallery}
            />
            {isPhotoQueueOpen && (
                <PhotoUploadQueue
                    photos={photos}
                    onRemove={removeFile}
                    onUpload={handleUpload}
                />
            )}
        </main>

    );
}

export default Welcome;