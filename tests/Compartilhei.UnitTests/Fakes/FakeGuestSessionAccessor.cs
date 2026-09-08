using Compartilhei.Application.Abstractions.Identity;

namespace Compartilhei.UnitTests.Fakes;

public sealed class FakeGuestSessionAccessor : IGuestSessionAccessor
{
    public Guid GuestSessionId { get; }

    public FakeGuestSessionAccessor(Guid guestSessionId)
    {
        GuestSessionId = guestSessionId;
    }
}