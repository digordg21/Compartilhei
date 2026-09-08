using System;

namespace Compartilhei.Domain.Entities
{
    public sealed class Favorite
    {
        public Guid Id { get; private set; }

        public Guid PhotoId { get; private set; }

        public Guid GuestSessionId { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public Photo Photo { get; private set; } = null!;

        public Favorite(
    Guid photoId,
    Guid guestSessionId)
        {
            if (photoId == Guid.Empty)
                throw new ArgumentException(
                    "Photo is required.",
                    nameof(photoId));

            if (guestSessionId == Guid.Empty)
                throw new ArgumentException(
                    "Guest session is required.",
                    nameof(guestSessionId));

            Id = Guid.NewGuid();
            PhotoId = photoId;
            GuestSessionId = guestSessionId;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        private Favorite()
        {
        }
    }
}