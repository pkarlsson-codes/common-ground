using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.Today.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.IntegrationTests.Homeownership;

[Collection(IntegrationCollection.Name)]
public class InsuranceCertificatesTests(ApiFactory factory)
{
    [Fact]
    public async Task RecordingCertificates_CoversTheLeaseAndMovesItsTodayItems()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateLease(client);

        (await TodayItems(client, leaseId)).Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Area = "stewardship",
            Kind = "insurance-certificate-due",
            RecordType = "ground-lease",
            Detail = "Dana Whitfield: no certificate on file",
            DueDate = (DateOnly?)new DateOnly(2018, 1, 1),
            Actions = new[] { "record-insurance-certificate" },
        });

        var shortOne = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/insurance-certificates",
            Certificate(today, trustNamed: false));
        shortOne.StatusCode.Should().Be(HttpStatusCode.Created);

        var lease = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        lease!.Insurance.Should().BeEquivalentTo(new
        {
            CoveredThrough = (DateOnly?)today.AddYears(1),
            CertificateDueOn = today.AddYears(1),
            CurrentCheck = new
            {
                MeetsLease = false,
                Shortfalls = new[] { "TrustNotAdditionalInsured" },
                ReplacementValue = (decimal?)280_000m,
                LiabilityPerOccurrence = (decimal?)300_000m,
            },
            Certificates = new[]
            {
                new { Insurer = "Acme Mutual", PolicyNumber = "HO-1234", PolicyEnd = today.AddYears(1), TrustNamedAdditionalInsured = false, RemovedReason = (string?)null },
            },
        });
        (await TodayItems(client, leaseId)).Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Kind = "insurance-short-of-lease",
            DueDate = (DateOnly?)today,
            Actions = Array.Empty<string>(),
        });

        var amended = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/insurance-certificates",
            Certificate(today, trustNamed: true));
        var amendedId = (await amended.Content.ReadFromJsonAsync<RecordInsuranceCertificateResponse>())!.CertificateId;
        (await TodayItems(client, leaseId)).Should().BeEmpty();

        var removed = await client.PostAsJsonAsync(
            $"/api/ground-leases/{leaseId}/insurance-certificates/{amendedId}/remove",
            new RemoveInsuranceCertificateRequest("Recorded on the wrong lease"));
        removed.StatusCode.Should().Be(HttpStatusCode.NoContent);

        lease = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        lease!.Insurance.Certificates.Single(c => c.CertificateId == amendedId).RemovedReason.Should().Be("Recorded on the wrong lease");
        lease.Insurance.CurrentCheck!.MeetsLease.Should().BeFalse();
        (await TodayItems(client, leaseId)).Select(i => i.Kind).Should().Equal("insurance-short-of-lease");
    }

    [Fact]
    public async Task RecordCertificate_PolicyEndBeforeStart_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateLease(client);

        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/insurance-certificates",
            Certificate(today, trustNamed: true) with { PolicyEnd = today.AddDays(-1) });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().StartWith("Policy end (");
    }

    [Fact]
    public async Task RecordCertificate_AdditionalInsuredLeftOut_Returns400()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateLease(client);

        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/insurance-certificates", new
        {
            insurer = "Acme Mutual",
            policyNumber = "HO-1234",
            policyStart = today,
            policyEnd = today.AddYears(1),
            dwellingCoverage = 280_000m,
            liabilityPerOccurrence = 300_000m,
            receivedOn = today,
            notes = (string?)null,
            documentId = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static RecordInsuranceCertificateRequest Certificate(DateOnly today, bool trustNamed) =>
        new("Acme Mutual", "HO-1234", today, today.AddYears(1), 280_000m, 300_000m, trustNamed, today, null, null);

    private static async Task<List<WorkItemResponse>> TodayItems(HttpClient client, string leaseId) =>
        [.. (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Items.Where(i => i.RecordId == leaseId && i.Kind.StartsWith("insurance-", StringComparison.Ordinal))];

    private static async Task<string> CreateLease(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/ground-leases",
            new CreateGroundLeaseRequest(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "Dana Whitfield", "dana@example.com", "(503) 555-0123",
                new DateOnly(2018, 1, 1), 99, 60, 0m, 0m, 200_000m, 250_000m, "Standard covenant", "AppraisalBased",
                0.25m, null, null, null, null, 90,
                InsuranceReplacementValue: 280_000m, InsuranceLiabilityPerOccurrence: 300_000m));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateGroundLeaseResponse>())!.GroundLeaseId;
    }
}
