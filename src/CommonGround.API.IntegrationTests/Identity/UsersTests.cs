using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.Identity;

[Collection(IntegrationCollection.Name)]
public class UsersTests(ApiFactory factory)
{
    [Fact]
    public async Task EnsureUser_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/users/ensure",
            new EnsureUserRequest("some-keycloak-sub", null, null, TrustId: "trust-testdefault00000000000"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EnsureUser_FirstCall_ReturnsUserId()
    {
        var client = factory.CreateClient().WithAuthToken();
        var subject = $"test-subject-{Guid.NewGuid()}";

        var response = await client.PostAsJsonAsync("/api/users/ensure",
            new EnsureUserRequest(subject, "test@example.com", "Test User", TrustId: "trust-testdefault00000000000"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<EnsureUserResponse>();
        result!.UserId.Should().NotBeNullOrWhiteSpace();
        result.UserId.Should().StartWith("user-");
    }

    [Fact]
    public async Task EnsureUser_SecondCall_ReturnsSameUserId()
    {
        var client = factory.CreateClient().WithAuthToken();
        var subject = $"test-subject-idempotent-{Guid.NewGuid()}";

        var first = await client.PostAsJsonAsync("/api/users/ensure",
            new EnsureUserRequest(subject, "idempotent@example.com", "Idempotent User", TrustId: "trust-testdefault00000000000"));
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstResult = await first.Content.ReadFromJsonAsync<EnsureUserResponse>();

        var second = await client.PostAsJsonAsync("/api/users/ensure",
            new EnsureUserRequest(subject, "idempotent@example.com", "Idempotent User", TrustId: "trust-testdefault00000000000"));
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondResult = await second.Content.ReadFromJsonAsync<EnsureUserResponse>();

        secondResult!.UserId.Should().Be(firstResult!.UserId);
    }
}
