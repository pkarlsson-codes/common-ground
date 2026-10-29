using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.TrustManagement.Responses;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.TrustManagement;

[Collection(IntegrationCollection.Name)]
public class TrustsGetCurrentTests(ApiFactory factory)
{
    [Fact]
    public async Task GetCurrentTrust_TrustMember_ReturnsTrustWithTimeZone()
    {
        var client = factory.CreateClient().WithAuthToken();

        var response = await client.GetAsync("/api/trusts/current");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var trust = await response.Content.ReadFromJsonAsync<TrustResponse>();
        trust!.Id.Should().Be(ApiFactory.TestTrustId);
        trust.TimeZone.Should().Be(TestTrustInitialData.TimeZone);
    }
}
