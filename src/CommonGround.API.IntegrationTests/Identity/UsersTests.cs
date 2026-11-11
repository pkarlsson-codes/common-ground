using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.Identity.Application.Commands;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace CommonGround.API.IntegrationTests.Identity;

[Collection(IntegrationCollection.Name)]
public class UsersTests(ApiFactory factory)
{
    [Fact]
    public async Task EnsureUser_SignInWithANewNameAndEmail_KeepsThemForTheMembersList()
    {
        var client = factory.CreateClient().WithAuthToken(trustId: string.Empty);
        var subject = $"test-subject-renamed-{Guid.NewGuid()}";
        var first = await (await client.PostAsJsonAsync("/api/users/ensure", new EnsureUserRequest(subject, "old@example.com", "Old Name", TrustId: null)))
            .Content.ReadFromJsonAsync<EnsureUserResponse>();
        var trustId = TrustId.Generate();
        await factory.Services.GetRequiredService<IMessageBus>().InvokeAsync(new AssignUserToTrustIfNone(UserId.Create(first!.UserId), trustId));

        await client.PostAsJsonAsync("/api/users/ensure", new EnsureUserRequest(subject, "new@example.com", "New Name", TrustId: null));

        var members = await factory.CreateClient().WithAuthToken(trustId: trustId.Value).GetFromJsonAsync<TrustMemberListResponse>("/api/trust-members");
        members!.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(new { first.UserId, Name = "New Name", Email = "new@example.com" });
    }

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

    [Fact]
    public async Task EnsureUser_FirstCall_WithNullTrustId_CreatesTrustlessUser()
    {
        var client = factory.CreateClient().WithAuthToken(trustId: string.Empty);
        var subject = $"test-subject-no-trust-{Guid.NewGuid()}";

        var response = await client.PostAsJsonAsync("/api/users/ensure",
            new EnsureUserRequest(subject, "notrust@example.com", "No Trust User", TrustId: null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<EnsureUserResponse>();
        result!.UserId.Should().NotBeNullOrWhiteSpace();
        result.UserId.Should().StartWith("user-");
        result.TrustId.Should().BeNull();
    }

    [Fact]
    public async Task EnsureUser_ExistingTrustlessUser_WithNullTrustId_RoundTripsNullTrust()
    {
        var client = factory.CreateClient().WithAuthToken(trustId: string.Empty);
        var subject = $"test-subject-existing-no-trust-{Guid.NewGuid()}";

        var first = await client.PostAsJsonAsync("/api/users/ensure",
            new EnsureUserRequest(subject, "existing-notrust@example.com", "Existing No Trust User", TrustId: null));
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstResult = await first.Content.ReadFromJsonAsync<EnsureUserResponse>();

        var second = await client.PostAsJsonAsync("/api/users/ensure",
            new EnsureUserRequest(subject, "existing-notrust@example.com", "Existing No Trust User", TrustId: null));
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondResult = await second.Content.ReadFromJsonAsync<EnsureUserResponse>();

        secondResult!.UserId.Should().Be(firstResult!.UserId);
        secondResult.TrustId.Should().BeNull();
    }
}
