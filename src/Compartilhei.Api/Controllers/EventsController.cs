using Compartilhei.Application.Events.CreateAlbum;
using Compartilhei.Application.Events.CreateEvent;
using Compartilhei.Application.Events.GetEventAlbums;
using Compartilhei.Application.Events.GetEventBySlug;
using Microsoft.AspNetCore.Mvc;

namespace Compartilhei.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    private readonly CreateEventHandler _createEventHandler;
    private readonly GetEventBySlugHandler _getEventBySlugHandler;
    private readonly CreateAlbumHandler _createAlbumHandler;
    private readonly GetEventAlbumsHandler _getEventAlbumsHandler;

    public EventsController(CreateEventHandler createEventHandler, 
        GetEventBySlugHandler getEventBySlugHandler,
        CreateAlbumHandler createAlbumHandler, GetEventAlbumsHandler getEventAlbumsHandler)
    {
        _createEventHandler = createEventHandler;
        _getEventBySlugHandler = getEventBySlugHandler;
        _createAlbumHandler = createAlbumHandler;
        _getEventAlbumsHandler = getEventAlbumsHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEventRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateEventCommand(
            request.Name,
            request.Slug);

        var result = await _createEventHandler.HandleAsync(
            command,
            cancellationToken);

        return Created(
            $"/api/events/{result.Id}",
            result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var query = new GetEventBySlugQuery(slug);

        var result = await _getEventBySlugHandler.HandleAsync(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{eventId:guid}/albums")]
    public async Task<IActionResult> CreateAlbum(
    Guid eventId,
    [FromBody] CreateAlbumRequest request,
    CancellationToken cancellationToken)
    {
        var command = new CreateAlbumCommand(
            eventId,
            request.Name,
            request.DisplayOrder);

        var result = await _createAlbumHandler.HandleAsync(
            command,
            cancellationToken);

        return Created(
            $"/api/events/{eventId}/albums/{result.Id}",
            result);
    }

    [HttpGet("{eventId:guid}/albums")]
    public async Task<IActionResult> GetAlbums(
    Guid eventId,
    CancellationToken cancellationToken)
    {
        var query = new GetEventAlbumsQuery(eventId);

        var result = await _getEventAlbumsHandler.HandleAsync(
            query,
            cancellationToken);

        return Ok(result);
    }
}

public sealed record CreateEventRequest(
    string Name,
    string Slug);

public sealed record CreateAlbumRequest(
    string Name,
    int DisplayOrder);