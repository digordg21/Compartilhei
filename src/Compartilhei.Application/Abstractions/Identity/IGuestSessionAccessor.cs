namespace Compartilhei.Application.Abstractions.Identity;

public interface IGuestSessionAccessor
{
    Guid GuestSessionId { get; }
}