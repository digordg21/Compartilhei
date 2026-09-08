using Compartilhei.Domain.Entities;

namespace Compartilhei.Application.Abstractions.Persistence;

public interface IEventRepository
{
    Task AddAsync(
        Event eventEntity,
        CancellationToken cancellationToken);

    Task<Event?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken);

    Task<Event?> GetByIdAsync(
    Guid eventId,
    CancellationToken cancellationToken);
}