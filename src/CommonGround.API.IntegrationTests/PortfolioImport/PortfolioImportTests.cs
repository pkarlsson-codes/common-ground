using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.PortfolioImport;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.PortfolioImport;

[Collection(IntegrationCollection.Name)]
public class PortfolioImportTests(ApiFactory factory)
{
    private const string Header =
        "Parcel Number,Parcel Address,Parcel Acres,Acquired On,Acquired By,Unit,Home Type,Bedrooms,Bathrooms,Accessible," +
        "Square Feet,Year Built,Assessed Value,Assessed On,Homeowner Name,Homeowner Email,Lease Template,Lease Start," +
        "Base Price,Appraised Value at Purchase,Fees Tracked From,Amount Owed Then";

    [Fact]
    public async Task Import_FileWithAnError_ImportsNothingAndListsTheError()
    {
        var client = factory.CreateClient().WithAuthToken();
        var tag = Guid.NewGuid().ToString("N")[..8];
        var template = await CreateTemplate(client, $"Lease {tag}");
        var csv = $"{Header}\n" +
                  $"APN-{tag},1 Elm St,0.2,2004-03-15,Donation,,SingleFamily,two,1,no,1100,1998,180000,2026-01-01,Ana {tag},ana@example.com,{template},2005-06-01,120000,150000,2026-01-01,0\n";

        var check = await Post(client, "/api/portfolio-import/check", csv);
        var import = await client.PostAsJsonAsync("/api/portfolio-import", new PortfolioImportRequest(csv));

        check.Errors.Should().ContainSingle().Which.Should().Be(new PortfolioImportErrorDto(2, "Bedrooms", "\"two\" is not a whole number."));
        import.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await Leases(client, tag)).Should().BeEmpty();
    }

    [Fact]
    public async Task Import_TwoHomesOnOneParcel_CreatesThemOnceAndASecondImportSkipsThem()
    {
        var client = factory.CreateClient().WithAuthToken();
        var tag = Guid.NewGuid().ToString("N")[..8];
        var template = await CreateTemplate(client, $"Lease {tag}");
        var csv = $"{Header}\n" +
                  $"APN-{tag},1 Elm St,0.2,3/15/2004,Donation,A,Duplex,3,1.5,no,\"1,100\",1998,\"$180,000\",2026-01-01,Ana {tag},ana@example.com,{template},6/1/2005,120000,150000,1/1/2026,$150\n" +
                  $"APN-{tag},1 Elm St,0.2,3/15/2004,Donation,B,Duplex,2,1,yes,900,1998,160000,2026-01-01,,,,,,,,\n";

        var check = await Post(client, "/api/portfolio-import/check", csv);
        var first = await Post(client, "/api/portfolio-import", csv);
        var second = await Post(client, "/api/portfolio-import", csv);

        (check.ParcelsToCreate, check.HomesToCreate, check.LeasesToCreate, check.Imported).Should().Be((1, 2, 1, false));
        (first.ParcelsToCreate, first.HomesToCreate, first.LeasesToCreate, first.Imported).Should().Be((1, 2, 1, true));
        (second.ParcelsToCreate, second.HomesToCreate, second.LeasesToCreate).Should().Be((0, 0, 0));
        (second.ParcelsExisting, second.HomesExisting, second.LeasesExisting).Should().Be((1, 2, 1));

        var lease = (await Leases(client, tag)).Should().ContainSingle().Subject;
        var detail = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{lease.Id}");
        detail!.LeaseFees.OpeningBalance.Should().Be(150m);
        detail.LeaseFees.FeesTrackedFrom.Day.Should().Be(1);
    }

    private static async Task<PortfolioImportResponse> Post(HttpClient client, string url, string csv)
    {
        var response = await client.PostAsJsonAsync(url, new PortfolioImportRequest(csv));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PortfolioImportResponse>())!;
    }

    private static async Task<IReadOnlyList<GroundLeaseListItemResponse>> Leases(HttpClient client, string tag)
    {
        var result = await client.GetFromJsonAsync<PaginatedResponse<GroundLeaseListItemResponse>>($"/api/ground-leases?search={tag}");
        return [.. result!.Items];
    }

    private static async Task<string> CreateTemplate(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/ground-lease-templates",
            new CreateGroundLeaseTemplateRequest(name, 99, 60, 45m, 15m, "Standard covenant", "AppraisalBased", 0.25m, null, null, null, null, 90));
        response.EnsureSuccessStatusCode();
        return name;
    }
}
