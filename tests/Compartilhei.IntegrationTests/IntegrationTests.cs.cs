using Compartilhei.IntegrationTests.Infrastructure;

namespace Compartilhei.IntegrationTests;

public class IntegrationTests
{
    [Fact]
    public async Task Should_start_api_successfully()
    {
        await using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }
}