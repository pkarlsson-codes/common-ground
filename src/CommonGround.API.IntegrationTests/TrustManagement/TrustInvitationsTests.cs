using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.IntegrationTests.TrustManagement;

/// <summary>The rules on invite links. The full invite-and-join flow is in Flows/TrustInvitationFlowTests.</summary>
[Collection(IntegrationCollection.Name)]
public class TrustInvitationsTests(ApiFactory factory)
{
    [Fact]
    public async Task CreateInvitation_WithABadEmail_Returns422()
    {
        var response = await factory.CreateClient().WithAuthToken()
            .PostAsJsonAsync("/api/trust-invitations", new CreateTrustInvitationRequest("not an email"));

        await ShouldBeViolationAsync(response, "shared.email.invalid_format");
    }

    [Fact]
    public async Task CreateInvitation_WithoutATrust_Returns403()
    {
        var response = await factory.CreateClient().WithAuthToken(trustId: string.Empty)
            .PostAsJsonAsync("/api/trust-invitations", new CreateTrustInvitationRequest("someone@example.org"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AcceptAndPreview_WithAnUnknownToken_Return422NoLongerValid()
    {
        var client = factory.CreateClient().WithAuthToken(trustId: string.Empty);
        var request = new TrustInvitationTokenRequest("not-a-real-token");

        await ShouldBeViolationAsync(await client.PostAsJsonAsync("/api/trust-invitations/preview", request), "trust_management.trust_invitation.accept.no_longer_valid");
        await ShouldBeViolationAsync(await client.PostAsJsonAsync("/api/trust-invitations/accept", request), "trust_management.trust_invitation.accept.no_longer_valid");
    }

    [Fact]
    public async Task Accept_ByAUserAlreadyInATrust_Returns422AndLeavesTheInviteUsable()
    {
        var invite = await CreateInvitationAsync(factory.CreateClient().WithAuthToken());

        var response = await factory.CreateClient().WithAuthToken(trustId: "trust-someotherone0000000000")
            .PostAsJsonAsync("/api/trust-invitations/accept", new TrustInvitationTokenRequest(invite.Token));

        await ShouldBeViolationAsync(response, "trust_management.trust_invitation.accept.already_in_trust");
        (await factory.CreateClient().WithAuthToken(trustId: string.Empty)
            .PostAsJsonAsync("/api/trust-invitations/preview", new TrustInvitationTokenRequest(invite.Token)))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cancel_StopsTheLinkWorkingAndRemovesItFromPending()
    {
        var member = factory.CreateClient().WithAuthToken();
        var invite = await CreateInvitationAsync(member);

        (await member.PostAsync($"/api/trust-invitations/{invite.InvitationId}/cancel", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await member.GetFromJsonAsync<TrustInvitationListResponse>("/api/trust-invitations"))!.Items
            .Should().NotContain(i => i.Id == invite.InvitationId);
        var accept = await factory.CreateClient().WithAuthToken(trustId: string.Empty)
            .PostAsJsonAsync("/api/trust-invitations/accept", new TrustInvitationTokenRequest(invite.Token));
        await ShouldBeViolationAsync(accept, "trust_management.trust_invitation.accept.no_longer_valid");
    }

    [Fact]
    public async Task Cancel_FromAnotherTrust_Returns422NotFound()
    {
        var invite = await CreateInvitationAsync(factory.CreateClient().WithAuthToken());

        var response = await factory.CreateClient().WithAuthToken(trustId: TrustId.Generate().Value)
            .PostAsync($"/api/trust-invitations/{invite.InvitationId}/cancel", null);

        await ShouldBeViolationAsync(response, "trust_management.trust_invitation.not_found");
    }

    private static async Task<CreateTrustInvitationResponse> CreateInvitationAsync(HttpClient member)
    {
        var response = await member.PostAsJsonAsync("/api/trust-invitations", new CreateTrustInvitationRequest($"{Guid.NewGuid():N}@example.org"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<CreateTrustInvitationResponse>())!;
    }

    private static async Task ShouldBeViolationAsync(HttpResponseMessage response, string code)
    {
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Extensions["code"]!.ToString().Should().Be(code);
    }
}
