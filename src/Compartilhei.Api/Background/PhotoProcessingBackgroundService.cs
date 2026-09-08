using Compartilhei.Application.Abstractions.Processing;
using Compartilhei.Application.Photos.Processing;

namespace Compartilhei.Api.Background;

public sealed class PhotoProcessingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IPhotoProcessingQueue _queue;
    private readonly ILogger<PhotoProcessingBackgroundService> _logger;

    public PhotoProcessingBackgroundService(
        IServiceScopeFactory scopeFactory,
        IPhotoProcessingQueue queue,
        ILogger<PhotoProcessingBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Photo processing background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var photoId = await _queue.DequeueAsync(
                    stoppingToken);

                await ProcessPhotoAsync(
                    photoId,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unexpected error in photo processing background service.");
            }
        }

        _logger.LogInformation(
            "Photo processing background service stopped.");
    }

    private async Task ProcessPhotoAsync(
        Guid photoId,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var handler =
            scope.ServiceProvider
                .GetRequiredService<ProcessPhotoHandler>();

        try
        {
            await handler.HandleAsync(
                new ProcessPhotoCommand(photoId),
                cancellationToken);

            _logger.LogInformation(
                "Photo {PhotoId} processed successfully.",
                photoId);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error processing photo {PhotoId}.",
                photoId);
        }
    }
}