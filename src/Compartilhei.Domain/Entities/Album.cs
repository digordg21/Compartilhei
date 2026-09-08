using Compartilhei.Domain.Entities;

public sealed class Album
{
    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public string Name { get; private set; } = null!;

    public int DisplayOrder { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Event Event { get; private set; } = null!;

    private readonly List<Photo> _photos = [];

    public IReadOnlyCollection<Photo> Photos => _photos.AsReadOnly();

    public Album(
    Guid eventId,
    string name,
    int displayOrder)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event is required.", nameof(eventId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Album name is required.", nameof(name));

        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder),
                "Display order cannot be negative.");

        Id = Guid.NewGuid();
        EventId = eventId;
        Name = name.Trim();
        DisplayOrder = displayOrder;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    private Album()
    {
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Album name is required.", nameof(name));

        Name = name.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder));

        DisplayOrder = displayOrder;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}