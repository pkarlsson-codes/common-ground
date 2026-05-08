using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.TrustManagement;

[Collection(IntegrationCollection.Name)]
public class TrustsCreateTests(ApiFactory factory)
{
    [Fact]
    public async Task CreateTrust_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/trusts",
            new CreateTrustRequest($"My Trust {Guid.NewGuid():N}"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTrust_TrustlessAuthenticatedUser_Returns200WithTrustId()
    {
        var subject = $"self-create-no-trust-{Guid.NewGuid():N}";

        var bootstrapClient = factory.CreateClient().WithAuthToken(trustId: string.Empty);

        var ensureResponse = await bootstrapClient.PostAsJsonAsync(
            "/api/users/ensure",
            new EnsureUserRequest(subject, "selfcreate@example.com", "Self Create", TrustId: null));

        ensureResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var ensurePayload = await ensureResponse.Content.ReadFromJsonAsync<EnsureUserResponse>();
        ensurePayload.Should().NotBeNull();

        var client = factory.CreateClient().WithAuthToken(userId: ensurePayload!.UserId, trustId: string.Empty);

        var response = await client.PostAsJsonAsync(
            "/api/trusts",
            new CreateTrustRequest($"Self Create Trust {Guid.NewGuid():N}"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<CreateTrustResponse>();
        payload.Should().NotBeNull();
        payload!.TrustId.Should().StartWith("trust-");
    }

    [Fact]
    public async Task CreateTrust_UserAlreadyHasTrust_Returns409()
    {
        var subject = $"self-create-existing-trust-{Guid.NewGuid():N}";
        var existingTrustId = "trust-testdefault00000000000";

        var bootstrapClient = factory.CreateClient().WithAuthToken(trustId: string.Empty);

        var ensureResponse = await bootstrapClient.PostAsJsonAsync(
            "/api/users/ensure",
            new EnsureUserRequest(subject, "existingtrust@example.com", "Existing Trust", TrustId: existingTrustId));

        ensureResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var ensurePayload = await ensureResponse.Content.ReadFromJsonAsync<EnsureUserResponse>();
        ensurePayload.Should().NotBeNull();

        var client = factory.CreateClient().WithAuthToken(userId: ensurePayload!.UserId, trustId: existingTrustId);

        var response = await client.PostAsJsonAsync(
            "/api/trusts",
            new CreateTrustRequest($"Blocked Trust {Guid.NewGuid():N}"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateTrust_DuplicateSubmitAfterSuccess_Returns409()
    {
        var subject = $"self-create-duplicate-{Guid.NewGuid():N}";
        var bootstrapClient = factory.CreateClient().WithAuthToken(trustId: string.Empty);

        var ensureResponse = await bootstrapClient.PostAsJsonAsync(
            "/api/users/ensure",
            new EnsureUserRequest(subject, "duplicate@example.com", "Duplicate Submit", TrustId: null));

        ensureResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var ensurePayload = await ensureResponse.Content.ReadFromJsonAsync<EnsureUserResponse>();
        ensurePayload.Should().NotBeNull();

        var client = factory.CreateClient().WithAuthToken(userId: ensurePayload!.UserId, trustId: string.Empty);

        var first = await client.PostAsJsonAsync(
            "/api/trusts",
            new CreateTrustRequest($"Duplicate Trust {Guid.NewGuid():N}"));

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstPayload = await first.Content.ReadFromJsonAsync<CreateTrustResponse>();
        firstPayload.Should().NotBeNull();
        firstPayload!.TrustId.Should().StartWith("trust-");

        var second = await client.PostAsJsonAsync(
            "/api/trusts",
            new CreateTrustRequest($"Duplicate Trust {Guid.NewGuid():N}"));

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}