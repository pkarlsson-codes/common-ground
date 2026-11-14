using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.Homeownership;

/// <summary>Covers the purchase board: open purchases in the column of the step they wait on.</summary>
[Collection(IntegrationCollection.Name)]
public class PurchaseBoardTests(ApiFactory factory)
{
    [Fact]
    public async Task GetPurchaseBoard_MovesAPurchaseToTheNextStepWhenOneIsRecorded()
    {
        var client = factory.CreateClient().WithAuthToken();
        var workflowId = await InitiateWorkflowAsync(client);

        var before = await GetBoardAsync(client);

        before.Columns.Select(c => c.Step).Should().Equal("Eligibility", "Appraisal", "Financing", "Inspection", "Title", "LeaseSigning", "Disclosure", "Closing");
        StepOf(before, workflowId).Should().Be("Eligibility");

        (await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/eligibility",
            new RecordEligibilityVerifiedRequest(before.Today, 50_000m, 1, ["doc-1"], null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var after = await GetBoardAsync(client);

        StepOf(after, workflowId).Should().Be("Appraisal");
        after.Columns.SelectMany(c => c.Cards).Should().ContainSingle(c => c.Id == workflowId)
            .Which.Should().BeEquivalentTo(new { BuyerFullName = "Board Buyer", Phase = "InProgress", StepEnteredOn = after.Today });
    }

    [Fact]
    public async Task GetPurchaseBoard_LeavesOutAWithdrawnPurchase()
    {
        var client = factory.CreateClient().WithAuthToken();
        var workflowId = await InitiateWorkflowAsync(client);
        var today = (await GetBoardAsync(client)).Today;

        (await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/withdraw",
            new WithdrawPurchaseRequest(today, "Buyer changed their mind")))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var board = await GetBoardAsync(client);

        board.Columns.SelectMany(c => c.Cards).Should().NotContain(c => c.Id == workflowId);
    }

    private static string? StepOf(PurchaseBoardResponse board, string workflowId) =>
        board.Columns.SingleOrDefault(c => c.Cards.Any(card => card.Id == workflowId))?.Step;

    private static async Task<PurchaseBoardResponse> GetBoardAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<PurchaseBoardResponse>("/api/purchase-workflows/board"))!;

    private static async Task<string> InitiateWorkflowAsync(HttpClient client)
    {
        var template = await client.PostAsJsonAsync("/api/ground-lease-templates",
            new CreateGroundLeaseTemplateRequest(
                $"Board Template {Guid.NewGuid():N}", 99, 60, 45m, 15m, "Standard covenant terms", "AppraisalBased", 0.25m, null, null, null, null, 90));
        template.StatusCode.Should().Be(HttpStatusCode.Created);
        var templateId = (await template.Content.ReadFromJsonAsync<CreateGroundLeaseTemplateResponse>())!.GroundLeaseTemplateId;

        var response = await client.PostAsJsonAsync("/api/purchase-workflows",
            new InitiatePurchaseWorkflowRequest(
                templateId, $"landparc-{Guid.NewGuid():N}", $"home-{Guid.NewGuid():N}", "Board Buyer", "board-buyer@example.com", null, 150000m));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<InitiatePurchaseWorkflowResponse>())!.PurchaseWorkflowId;
    }
}
