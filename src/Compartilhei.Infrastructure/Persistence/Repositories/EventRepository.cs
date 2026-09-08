using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Compartilhei.Infrastructure.Persistence.Repositories;

public sealed class EventRepository : IEventRepository
{
    private readonly CompartilheiDbContext _dbContext;

    public EventRepository(CompartilheiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Event eventEntity,
        CancellationToken cancellationToken)
    {
        await _dbContext.Events.AddAsync(
            eventEntity,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<Event?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Events
            .FirstOrDefaultAsync(
                x => x.Slug == slug,
                cancellationToken);
    }

    public async Task<Event?> GetByIdAsync(
    Guid eventId,
    CancellationToken cancellationToken)
    {
        return await _dbContext.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == eventId,
                cancellationToken);
    }
}