using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Domain.Entities;

namespace Compartilhei.UnitTests.Fakes;

public sealed class FakeEventRepository : IEventRepository
{
    private readonly List<Event> _events = [];

    public bool AddCalled { get; private set; }

    public void Seed(Event eventEntity)
    {
        _events.Add(eventEntity);
    }

    public Task AddAsync(
        Event eventEntity,
        CancellationToken cancellationToken)
    {
        AddCalled = true;
        _events.Add(eventEntity);

        return Task.CompletedTask;
    }

    public Task<Event?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        var eventEntity = _events.FirstOrDefault(
            x => x.Slug == slug);

        return Task.FromResult(eventEntity);
    }

    public Task<Event?> GetByIdAsync(
    Guid eventId,
    CancellationToken cancellationToken)
    {
        var eventEntity = _events.FirstOrDefault(
            x => x.Id == eventId);

        return Task.FromResult(eventEntity);
    }

    public void SeedWithId(Event eventEntity, Guid eventId)
    {
        // Somente para o teste conseguir associar o Event ao ID esperado.
        _events.Add(eventEntity);
    }
}