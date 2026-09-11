using Azure.Storage.Blobs;
using Compartilhei.Api.Background;
using Compartilhei.Api.ExceptionHandling;
using Compartilhei.Api.Identity;
using Compartilhei.Application.Abstractions.Identity;
using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Abstractions.Processing;
using Compartilhei.Application.Abstractions.Storage;
using Compartilhei.Application.Events.CreateAlbum;
using Compartilhei.Application.Events.CreateEvent;
using Compartilhei.Application.Events.GetEventAlbums;
using Compartilhei.Application.Events.GetEventBySlug;
using Compartilhei.Application.Photos.ConfirmUpload;
using Compartilhei.Application.Photos.Favorites;
using Compartilhei.Application.Photos.Favorites.GetFavorites;
using Compartilhei.Application.Photos.Gallery;
using Compartilhei.Application.Photos.Processing;
using Compartilhei.Application.Photos.RequestUpload;
using Compartilhei.Application.Photos.Validation;
using Compartilhei.Infrastructure.Configuration;
using Compartilhei.Infrastructure.Images;
using Compartilhei.Infrastructure.Images.Configuration;
using Compartilhei.Infrastructure.Persistence;
using Compartilhei.Infrastructure.Persistence.Repositories;
using Compartilhei.Infrastructure.Processing;
using Compartilhei.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;





var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();

var connectionString = builder.Configuration.GetConnectionString(
    "DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");
}

builder.Services.AddDbContext<CompartilheiDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<CompartilheiDbContext>();

builder.Services
    .AddOptions<AzureStorageOptions>()
    .Bind(builder.Configuration.GetSection(AzureStorageOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ConnectionString),
        "AzureStorage:ConnectionString é obrigatório.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ContainerName),
        "AzureStorage:ContainerName é obrigatório.")
    .Validate(
        options => options.UploadSasExpirationMinutes > 0,
        "AzureStorage:UploadSasExpirationMinutes deve ser maior que zero.")
    .Validate(
        options => options.ReadSasExpirationMinutes > 0,
        "AzureStorage:ReadSasExpirationMinutes deve ser maior que zero.")
    .ValidateOnStart();

builder.Services
    .AddOptions<ImageProcessingOptions>()
    .Bind(builder.Configuration.GetSection(
        ImageProcessingOptions.SectionName))
    .Validate(
        options => options.DisplayMaxWidth > 0,
        "ImageProcessing:DisplayMaxWidth deve ser maior que zero.")
    .Validate(
        options => options.DisplayMaxHeight > 0,
        "ImageProcessing:DisplayMaxHeight deve ser maior que zero.")
    .Validate(
        options => options.ThumbnailMaxWidth > 0,
        "ImageProcessing:ThumbnailMaxWidth deve ser maior que zero.")
    .Validate(
        options => options.ThumbnailMaxHeight > 0,
        "ImageProcessing:ThumbnailMaxHeight deve ser maior que zero.")
    .ValidateOnStart();

builder.Services.AddHostedService<PhotoProcessingBackgroundService>();

builder.Services.AddSingleton(sp =>
{
    var options = sp
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureStorageOptions>>()
        .Value;

    return new BlobServiceClient(options.ConnectionString);
});

builder.Services.AddSingleton<IPhotoProcessingQueue, InMemoryPhotoProcessingQueue>();

builder.Services.AddScoped<IPhotoStorage, AzureBlobPhotoStorage>();

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IGuestSessionAccessor, GuestSessionAccessor>();
builder.Services.AddScoped<IPhotoFileStorage, AzureBlobPhotoFileStorage>();
builder.Services.AddScoped<IPhotoProcessor, ImageSharpPhotoProcessor>();
builder.Services.AddScoped<RecoverPhotoProcessingHandler>();
builder.Services.AddScoped<CreateEventHandler>();
builder.Services.AddScoped<GetEventBySlugHandler>();
builder.Services.AddScoped<CreateAlbumHandler>();
builder.Services.AddScoped<GetEventAlbumsHandler>();
builder.Services.AddScoped<RequestUploadHandler>();
builder.Services.AddScoped<ConfirmUploadHandler>();
builder.Services.AddScoped<PhotoUploadValidator>();
builder.Services.AddScoped<ProcessPhotoHandler>();
builder.Services.AddScoped<GetPhotoGalleryHandler>();
builder.Services.AddScoped<FavoritePhotoHandler>();
builder.Services.AddScoped<UnfavoritePhotoHandler>();
builder.Services.AddScoped<GetFavoritesHandler>();


builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapControllers();

app.MapHealthChecks("/health");
app.Run();