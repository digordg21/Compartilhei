using Azure.Storage.Blobs;
using Compartilhei.Api.Contracts.Photos.Favorites.GetFavorites;
using Compartilhei.Application.Photos.Favorites.GetFavorites;
using Compartilhei.Domain.Entities;
using Compartilhei.Domain.Enums;
using Compartilhei.Infrastructure.Configuration;
using Compartilhei.Infrastructure.Persistence;
using Compartilhei.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;

namespace Compartilhei.IntegrationTests.Photos;

public class FavoritePhotoIntegrationTests
{
    [Fact]
    public async Task Should_favorite_photo_through_api()
    {
        await using var factory = new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var eventEntity = new Event(
            "Evento Integration Test",
            $"evento-test-{Guid.NewGuid():N}");

        var album = new Album(
            eventEntity.Id,
            "Álbum Integration Test",
            1);

        var guestSessionId = Guid.NewGuid();

        var photo = new Photo(
            album.Id,
            "foto-integration.jpg",
            1_000,
            guestSessionId);

        photo.MarkUploaded(
            $"events/{eventEntity.Id}/albums/{album.Id}/original/{photo.Id}.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        photo.MarkAvailable(
            $"events/{eventEntity.Id}/albums/{album.Id}/display/{photo.Id}.jpg",
            $"events/{eventEntity.Id}/albums/{album.Id}/thumbnail/{photo.Id}.jpg");

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<CompartilheiDbContext>();

            await dbContext.Events.AddAsync(eventEntity);
            await dbContext.Albums.AddAsync(album);
            await dbContext.Photos.AddAsync(photo);

            await dbContext.SaveChangesAsync();
        }

        var response = await client.PostAsync(
            $"/api/events/{eventEntity.Slug}" +
            $"/albums/{album.Id}" +
            $"/photos/{photo.Id}" +
            "/favorite",
            null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<FavoriteResponse>();

        Assert.NotNull(result);
        Assert.True(result.IsFavorited);
        Assert.Equal(1, result.FavoriteCount);
    }

    [Fact]
    public async Task Should_not_duplicate_favorite_through_api()
    {
        await using var factory = new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var eventEntity = new Event(
            "Evento Integration Test",
            $"evento-test-{Guid.NewGuid():N}");

        var album = new Album(
            eventEntity.Id,
            "Álbum Integration Test",
            1);

        var photo = new Photo(
            album.Id,
            "foto-integration.jpg",
            1_000,
            Guid.NewGuid());

        photo.MarkUploaded(
            $"events/{eventEntity.Id}/albums/{album.Id}/original/{photo.Id}.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        photo.MarkAvailable(
            $"events/{eventEntity.Id}/albums/{album.Id}/display/{photo.Id}.jpg",
            $"events/{eventEntity.Id}/albums/{album.Id}/thumbnail/{photo.Id}.jpg");

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<CompartilheiDbContext>();

            await dbContext.Events.AddAsync(eventEntity);
            await dbContext.Albums.AddAsync(album);
            await dbContext.Photos.AddAsync(photo);

            await dbContext.SaveChangesAsync();
        }

        var requestUri =
            $"/api/events/{eventEntity.Slug}" +
            $"/albums/{album.Id}" +
            $"/photos/{photo.Id}" +
            "/favorite";

        var firstResponse = await client.PostAsync(requestUri, null);

        var firstResult =
            await firstResponse.Content.ReadFromJsonAsync<FavoriteResponse>();

        var secondResponse = await client.PostAsync(requestUri, null);

        var secondResult =
            await secondResponse.Content.ReadFromJsonAsync<FavoriteResponse>();

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        Assert.NotNull(firstResult);
        Assert.NotNull(secondResult);

        Assert.True(firstResult.IsFavorited);
        Assert.Equal(1, firstResult.FavoriteCount);

        Assert.True(secondResult.IsFavorited);
        Assert.Equal(1, secondResult.FavoriteCount);
    }

    [Fact]
    public async Task Should_unfavorite_photo_through_api()
    {
        await using var factory = new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var eventEntity = new Event(
            "Evento Integration Test",
            $"evento-test-{Guid.NewGuid():N}");

        var album = new Album(
            eventEntity.Id,
            "Álbum Integration Test",
            1);

        var photo = new Photo(
            album.Id,
            "foto-integration.jpg",
            1_000,
            Guid.NewGuid());

        photo.MarkUploaded(
            $"events/{eventEntity.Id}/albums/{album.Id}/original/{photo.Id}.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        photo.MarkAvailable(
            $"events/{eventEntity.Id}/albums/{album.Id}/display/{photo.Id}.jpg",
            $"events/{eventEntity.Id}/albums/{album.Id}/thumbnail/{photo.Id}.jpg");

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<CompartilheiDbContext>();

            await dbContext.Events.AddAsync(eventEntity);
            await dbContext.Albums.AddAsync(album);
            await dbContext.Photos.AddAsync(photo);

            await dbContext.SaveChangesAsync();
        }

        var requestUri =
            $"/api/events/{eventEntity.Slug}" +
            $"/albums/{album.Id}" +
            $"/photos/{photo.Id}" +
            "/favorite";

        await client.PostAsync(requestUri, null);

        var deleteResponse =
            await client.DeleteAsync(requestUri);

        var result =
            await deleteResponse.Content
                .ReadFromJsonAsync<FavoriteResponse>();

        Assert.Equal(
            HttpStatusCode.OK,
            deleteResponse.StatusCode);

        Assert.NotNull(result);

        Assert.False(result.IsFavorited);
        Assert.Equal(0, result.FavoriteCount);
    }

    [Fact]
    public async Task Should_not_fail_when_unfavoriting_non_existing_favorite()
    {
        await using var factory = new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var eventEntity = new Event(
            "Evento Integration Test",
            $"evento-test-{Guid.NewGuid():N}");

        var album = new Album(
            eventEntity.Id,
            "Álbum Integration Test",
            1);

        var photo = new Photo(
            album.Id,
            "foto-integration.jpg",
            1_000,
            Guid.NewGuid());

        photo.MarkUploaded(
            $"events/{eventEntity.Id}/albums/{album.Id}/original/{photo.Id}.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        photo.MarkAvailable(
            $"events/{eventEntity.Id}/albums/{album.Id}/display/{photo.Id}.jpg",
            $"events/{eventEntity.Id}/albums/{album.Id}/thumbnail/{photo.Id}.jpg");

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<CompartilheiDbContext>();

            await dbContext.Events.AddAsync(eventEntity);
            await dbContext.Albums.AddAsync(album);
            await dbContext.Photos.AddAsync(photo);

            await dbContext.SaveChangesAsync();
        }

        var requestUri =
            $"/api/events/{eventEntity.Slug}" +
            $"/albums/{album.Id}" +
            $"/photos/{photo.Id}" +
            "/favorite";

        var deleteResponse =
            await client.DeleteAsync(requestUri);

        var result =
            await deleteResponse.Content
                .ReadFromJsonAsync<FavoriteResponse>();

        Assert.Equal(
            HttpStatusCode.OK,
            deleteResponse.StatusCode);

        Assert.NotNull(result);

        Assert.False(result.IsFavorited);
        Assert.Equal(0, result.FavoriteCount);
    }

    [Fact]
    public async Task Should_return_favorite_for_current_guest_session()
    {
        await using var factory = new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var eventEntity = new Event(
            "Evento Integration Test",
            $"evento-test-{Guid.NewGuid():N}");

        var album = new Album(
            eventEntity.Id,
            "Álbum Integration Test",
            1);

        var photo = new Photo(
            album.Id,
            "foto-integration.jpg",
            1_000,
            Guid.NewGuid());

        photo.MarkUploaded(
            $"events/{eventEntity.Id}/albums/{album.Id}/original/{photo.Id}.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        photo.MarkAvailable(
            $"events/{eventEntity.Id}/albums/{album.Id}/display/{photo.Id}.jpg",
            $"events/{eventEntity.Id}/albums/{album.Id}/thumbnail/{photo.Id}.jpg");

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<CompartilheiDbContext>();

            await dbContext.Events.AddAsync(eventEntity);
            await dbContext.Albums.AddAsync(album);
            await dbContext.Photos.AddAsync(photo);

            await dbContext.SaveChangesAsync();
        }

        var favoriteUri =
            $"/api/events/{eventEntity.Slug}" +
            $"/albums/{album.Id}" +
            $"/photos/{photo.Id}" +
            "/favorite";

        var favoriteResponse =
            await client.PostAsync(favoriteUri, null);

        favoriteResponse.EnsureSuccessStatusCode();

        var favoritesUri =
            $"/api/events/{eventEntity.Slug}" +
            $"/albums/{album.Id}" +
            "/photos/favorites";

        var response =
            await client.GetAsync(favoritesUri);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<GetFavoritesResponse>();

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(photo.Id, result.Items[0].PhotoId);
    }

    [Fact]
    public async Task Should_not_return_favorite_from_another_guest_session()
    {
        await using var factory = new CustomWebApplicationFactory();

        using var clientA = factory.CreateClient();
        using var clientB = factory.CreateClient();

        var eventEntity = new Event(
            "Evento Integration Test",
            $"evento-test-{Guid.NewGuid():N}");

        var album = new Album(
            eventEntity.Id,
            "Álbum Integration Test",
            1);

        var photo = new Photo(
            album.Id,
            "foto-integration.jpg",
            1_000,
            Guid.NewGuid());

        photo.MarkUploaded(
            $"events/{eventEntity.Id}/albums/{album.Id}/original/{photo.Id}.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        photo.MarkAvailable(
            $"events/{eventEntity.Id}/albums/{album.Id}/display/{photo.Id}.jpg",
            $"events/{eventEntity.Id}/albums/{album.Id}/thumbnail/{photo.Id}.jpg");

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<CompartilheiDbContext>();

            await dbContext.Events.AddAsync(eventEntity);
            await dbContext.Albums.AddAsync(album);
            await dbContext.Photos.AddAsync(photo);

            await dbContext.SaveChangesAsync();
        }

        var favoriteUri =
            $"/api/events/{eventEntity.Slug}" +
            $"/albums/{album.Id}" +
            $"/photos/{photo.Id}" +
            "/favorite";

        var favoriteResponse =
            await clientA.PostAsync(favoriteUri, null);

        favoriteResponse.EnsureSuccessStatusCode();

        var favoritesUri =
            $"/api/events/{eventEntity.Slug}" +
            $"/albums/{album.Id}" +
            "/photos/favorites";

        var response =
            await clientB.GetAsync(favoritesUri);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<FavoriteListResponse>();

        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Should_download_favorites_as_zip()
    {
        await using var factory = new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var eventEntity = new Event(
            "Evento Integration Test",
            $"evento-test-{Guid.NewGuid():N}");

        var album = new Album(
            eventEntity.Id,
            "Álbum Integration Test",
            1);

        var photo = new Photo(
            album.Id,
            "foto-integration.jpg",
            1_000,
            Guid.NewGuid());

        photo.MarkUploaded(
            $"events/{eventEntity.Id}/albums/{album.Id}/original/{photo.Id}.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        photo.MarkAvailable(
            $"events/{eventEntity.Id}/albums/{album.Id}/display/{photo.Id}.jpg",
            $"events/{eventEntity.Id}/albums/{album.Id}/thumbnail/{photo.Id}.jpg");

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<CompartilheiDbContext>();

            await dbContext.Events.AddAsync(eventEntity);
            await dbContext.Albums.AddAsync(album);
            await dbContext.Photos.AddAsync(photo);

            await dbContext.SaveChangesAsync();

            await SeedBlobAsync(
                factory,
                photo.OriginalPath!);
        }

        var favoriteUri =
            $"/api/events/{eventEntity.Slug}" +
            $"/albums/{album.Id}" +
            $"/photos/{photo.Id}" +
            "/favorite";

        var favoriteResponse =
            await client.PostAsync(favoriteUri, null);

        favoriteResponse.EnsureSuccessStatusCode();

        var downloadUri =
            $"/api/events/{eventEntity.Slug}" +
            $"/albums/{album.Id}" +
            "/photos/favorites/download";

        var response =
            await client.GetAsync(downloadUri);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.Equal(
            "application/zip",
            response.Content.Headers.ContentType?.MediaType);

        Assert.NotNull(
            response.Content.Headers.ContentDisposition);

        Assert.Contains(
            "favoritos.zip",
            response.Content.Headers.ContentDisposition!.ToString());

        var content = await response.Content.ReadAsByteArrayAsync();

        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task Should_download_zip_containing_favorite_file()
    {
        await using var factory = new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var eventEntity = new Event(
            "Evento Integration Test",
            $"evento-test-{Guid.NewGuid():N}");

        var album = new Album(
            eventEntity.Id,
            "Álbum Integration Test",
            1);

        var photo = new Photo(
            album.Id,
            "foto-integration.jpg",
            1_000,
            Guid.NewGuid());

        photo.MarkUploaded(
            $"events/{eventEntity.Id}/albums/{album.Id}/original/{photo.Id}.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        photo.MarkAvailable(
            $"events/{eventEntity.Id}/albums/{album.Id}/display/{photo.Id}.jpg",
            $"events/{eventEntity.Id}/albums/{album.Id}/thumbnail/{photo.Id}.jpg");

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<CompartilheiDbContext>();

            await dbContext.Events.AddAsync(eventEntity);
            await dbContext.Albums.AddAsync(album);
            await dbContext.Photos.AddAsync(photo);

            await dbContext.SaveChangesAsync();

            await SeedBlobAsync(
                factory,
                photo.OriginalPath!);
        }

        var favoriteUri =
            $"/api/events/{eventEntity.Slug}" +
            $"/albums/{album.Id}" +
            $"/photos/{photo.Id}" +
            "/favorite";

        await client.PostAsync(
            favoriteUri,
            null);

        var downloadUri =
            $"/api/events/{eventEntity.Slug}" +
            $"/albums/{album.Id}" +
            "/photos/favorites/download";

        var response =
            await client.GetAsync(downloadUri);

        response.EnsureSuccessStatusCode();

        var content =
            await response.Content.ReadAsByteArrayAsync();

        using var zipStream =
            new MemoryStream(content);

        using var archive =
            new ZipArchive(
                zipStream,
                ZipArchiveMode.Read);

        Assert.Single(archive.Entries);

        Assert.Equal(
            "foto-integration.jpg",
            archive.Entries[0].Name);
    }

    private sealed record FavoriteResponse(
        bool IsFavorited,
        int FavoriteCount);

    private sealed record FavoriteListResponse(
    IReadOnlyList<FavoriteListItemResponse> Items);

    private sealed record FavoriteListItemResponse(
        Guid PhotoId,
        string FileName,
        string ThumbnailUrl,
        string DisplayUrl,
        int Width,
        int Height,
        DateTimeOffset CreatedAt);

    private static async Task SeedBlobAsync(
    WebApplicationFactory<Program> factory,
    string blobPath)
    {
        using var scope = factory.Services.CreateScope();

        var blobServiceClient = scope.ServiceProvider
            .GetRequiredService<BlobServiceClient>();

        var options = scope.ServiceProvider
            .GetRequiredService<
                Microsoft.Extensions.Options.IOptions<AzureStorageOptions>>();

        var containerClient = blobServiceClient
            .GetBlobContainerClient(options.Value.ContainerName);

        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(blobPath);

        await blobClient.UploadAsync(
            BinaryData.FromBytes([1, 2, 3, 4]),
            overwrite: true);
    }
}