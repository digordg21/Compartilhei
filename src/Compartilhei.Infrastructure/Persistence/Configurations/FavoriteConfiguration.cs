using Compartilhei.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compartilhei.Infrastructure.Persistence.Configurations;

public sealed class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PhotoId)
            .IsRequired();

        builder.Property(x => x.GuestSessionId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Photo)
            .WithMany(x => x.Favorites)
            .HasForeignKey(x => x.PhotoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.PhotoId,
            x.GuestSessionId
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.GuestSessionId,
            x.CreatedAt
        });
    }
}