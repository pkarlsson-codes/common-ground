using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.IntegrationTests.Flows;

/// <summary>
/// A trust member invites a colleague, who joins with the link, and later removes them. Runs on the durable host,
/// so the membership check sees what a real request sees right after joining or removal.
/// </summary>
[Collection(DurableIntegrationCollection.Name)]
public class TrustInvitationFlowTests(DurableApiFactory factory)
{
    private readonly FlowTracker _tracker = new(factory);

    [Fact]
    public async Task AColleagueJoinsWithAnInviteLink_AndBothAreListedAsMembers()
    {
        var founderName = $"Founder {Guid.NewGuid():N}";
        var founderId = await EnsureUserAsync(founderName);
        var created = await _tracker.TrackAsync(async () =>
        {
            var response = await factory.CreateClient().WithAuthToken(userId: founderId, trustId: string.Empty)
                .PostAsJsonAsync("/api/trusts", new CreateTrustRequest($"Invite Flow CLT {Guid.NewGuid():N}", "America/Denver"));
            return (await response.Content.ReadFromJsonAsync<CreateTrustResponse>())!;
        });
        var founder = factory.CreateClient().WithAuthToken(userId: founderId, trustId: created.TrustId);

        var invite = await (await founder.PostAsJsonAsync("/api/trust-invitations", new CreateTrustInvitationRequest(" colleague@example.org ")))
            .Content.ReadFromJsonAsync<CreateTrustInvitationResponse>();
        (await founder.GetFromJsonAsync<TrustInvitationListResponse>("/api/trust-invitations"))!.Items
            .Should().ContainSingle().Which.Should().BeEquivalentTo(new { invite!.InvitationId, Id = invite.InvitationId, Email = "colleague@example.org", InvitedByName = founderName }, o => o.ExcludingMissingMembers());

        var colleagueName = $"Colleague {Guid.NewGuid():N}";
        var colleague = factory.CreateClient().WithAuthToken(userId: await EnsureUserAsync(colleagueName), trustId: string.Empty);
        (await (await colleague.PostAsJsonAsync("/api/trust-invitations/preview", new TrustInvitationTokenRequest(invite.Token)))
            .Content.ReadFromJsonAsync<TrustInvitationPreviewResponse>())!.Email.Should().Be("colleague@example.org");

        var joined = await _tracker.TrackAsync(async () =>
            (await (await colleague.PostAsJsonAsync("/api/trust-invitations/accept", new TrustInvitationTokenRequest(invite.Token)))
                .Content.ReadFromJsonAsync<AcceptTrustInvitationResponse>())!);

        joined.TrustId.Should().Be(created.TrustId);
        (await founder.GetFromJsonAsync<TrustMemberListResponse>("/api/trust-members"))!.Items
            .Select(m => m.Name).Should().BeEquivalentTo(founderName, colleagueName);
        (await founder.GetFromJsonAsync<TrustInvitationListResponse>("/api/trust-invitations"))!.Items.Should().BeEmpty();

        var someoneElse = factory.CreateClient().WithAuthToken(userId: await EnsureUserAsync("Someone Else"), trustId: string.Empty);
        var reused = await someoneElse.PostAsJsonAsync("/api/trust-invitations/accept", new TrustInvitationTokenRequest(invite.Token));
        reused.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await reused.Content.ReadFromJsonAsync<ProblemDetails>())!.Extensions["code"]!.ToString()
            .Should().Be("trust_management.trust_invitation.accept.no_longer_valid");
    }

    [Fact]
    public async Task ARemovedMember_LosesAccessOnTheNextRequest_AndCanBeInvitedAgain()
    {
        var founderId = await EnsureUserAsync($"Founder {Guid.NewGuid():N}");
        var created = await _tracker.TrackAsync(async () =>
        {
            var response = await factory.CreateClient().WithAuthToken(userId: founderId, trustId: string.Empty)
                .PostAsJsonAsync("/api/trusts", new CreateTrustRequest($"Removal Flow CLT {Guid.NewGuid():N}", "America/Denver"));
            return (await response.Content.ReadFromJsonAsync<CreateTrustResponse>())!;
        });
        var founder = factory.CreateClient().WithAuthToken(userId: founderId, trustId: created.TrustId);
        (await founder.GetAsync("/api/trusts/current")).StatusCode.Should().Be(HttpStatusCode.OK, "the creator is assigned before the response");

        var colleagueId = await EnsureUserAsync($"Colleague {Guid.NewGuid():N}");
        await JoinAsync(founder, colleagueId);
        var colleagueInTrust = factory.CreateClient().WithAuthToken(userId: colleagueId, trustId: created.TrustId);
        (await colleagueInTrust.GetAsync("/api/trusts/current")).StatusCode.Should().Be(HttpStatusCode.OK, "the colleague is assigned before the response");

        var removeSelf = await founder.PostAsync($"/api/trust-members/{founderId}/remove", null);
        removeSelf.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await removeSelf.Content.ReadFromJsonAsync<ProblemDetails>())!.Extensions["code"]!.ToString()
            .Should().Be("identity.user.remove_from_trust.cannot_remove_self");

        (await founder.PostAsync($"/api/trust-members/{colleagueId}/remove", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        // The colleague's session still names the trust; the API refuses it.
        var refused = await colleagueInTrust.GetAsync("/api/trusts/current");
        refused.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await refused.Content.ReadFromJsonAsync<ProblemDetails>())!.Extensions["code"]!.ToString()
            .Should().Be("identity.trust_membership.ended");
        (await founder.GetFromJsonAsync<TrustMemberListResponse>("/api/trust-members"))!.Items
            .Select(m => m.UserId).Should().BeEquivalentTo([founderId]);

        var removeAgain = await founder.PostAsync($"/api/trust-members/{colleagueId}/remove", null);
        (await removeAgain.Content.ReadFromJsonAsync<ProblemDetails>())!.Extensions["code"]!.ToString()
            .Should().Be("identity.user.remove_from_trust.not_a_member");

        await JoinAsync(founder, colleagueId);
        (await colleagueInTrust.GetAsync("/api/trusts/current")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task JoinAsync(HttpClient member, string userId)
    {
        var invite = await (await member.PostAsJsonAsync("/api/trust-invitations", new CreateTrustInvitationRequest("colleague@example.org")))
            .Content.ReadFromJsonAsync<CreateTrustInvitationResponse>();
        var joiner = factory.CreateClient().WithAuthToken(userId: userId, trustId: string.Empty);
        (await joiner.PostAsJsonAsync("/api/trust-invitations/accept", new TrustInvitationTokenRequest(invite!.Token)))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<string> EnsureUserAsync(string name)
    {
        var response = await factory.CreateClient().WithAuthToken(trustId: string.Empty).PostAsJsonAsync(
            "/api/users/ensure", new EnsureUserRequest($"invite-flow-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.org", name, TrustId: null));
        return (await response.Content.ReadFromJsonAsync<EnsureUserResponse>())!.UserId;
    }
}
