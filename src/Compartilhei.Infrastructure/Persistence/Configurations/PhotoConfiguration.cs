using Compartilhei.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compartilhei.Infrastructure.Persistence.Configurations;

public sealed class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.ToTable("Photos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AlbumId)
            .IsRequired();

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.OriginalPath)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(x => x.DisplayPath)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(x => x.ThumbnailPath)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.Width)
            .IsRequired(false);

        builder.Property(x => x.Height)
            .IsRequired(false);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.UploadedBySessionId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ProcessedAt);

        builder.HasOne(x => x.Album)
            .WithMany(x => x.Photos)
            .HasForeignKey(x => x.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.AlbumId,
            x.CreatedAt,
            x.Id
        });

        builder.HasIndex(x => x.UploadedBySessionId);
    }
}