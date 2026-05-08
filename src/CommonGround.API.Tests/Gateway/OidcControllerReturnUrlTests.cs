using CommonGround.API.Clients;
using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.Web.Gateway.Controllers;
using CommonGround.Web.Gateway.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace CommonGround.API.Tests.Gateway;

public sealed class OidcControllerReturnUrlTests
{
    [Fact]
    public void Login_WithValidRelativeReturnUrl_UsesReturnUrlAsRedirectUri()
    {
        var controller = BuildController("/invite/abc?x=1");

        var result = controller.Login();

        var challenge = Assert.IsType<ChallengeResult>(result);
        Assert.Equal(OpenIdConnectDefaults.AuthenticationScheme, Assert.Single(challenge.AuthenticationSchemes));
        Assert.Equal("/invite/abc?x=1", challenge.Properties?.RedirectUri);
    }

    [Theory]
    [InlineData("http://evil.example/path")]
    [InlineData("https://evil.example/path")]
    [InlineData("//evil.example/path")]
    public void Login_WithInvalidReturnUrl_UsesDefaultRedirect(string returnUrl)
    {
        var controller = BuildController(returnUrl);

        var result = controller.Login();

        var challenge = Assert.IsType<ChallengeResult>(result);
        Assert.Equal("/default", challenge.Properties?.RedirectUri);
    }

    [Fact]
    public void Login_WithoutReturnUrl_UsesDefaultRedirect()
    {
        var controller = BuildController();

        var result = controller.Login();

        var challenge = Assert.IsType<ChallengeResult>(result);
        Assert.Equal("/default", challenge.Properties?.RedirectUri);
    }

    [Fact]
    public void Register_WithValidRelativeReturnUrl_UsesReturnUrlAsRedirectUri()
    {
        var controller = BuildController("/invite/abc?x=1", "trust-123");

        var result = controller.Register();

        var challenge = Assert.IsType<ChallengeResult>(result);
        Assert.Equal(OpenIdConnectDefaults.AuthenticationScheme, Assert.Single(challenge.AuthenticationSchemes));
        Assert.Equal("/invite/abc?x=1", challenge.Properties?.RedirectUri);
        Assert.Equal("create", challenge.Properties?.Parameters["prompt"]);
    }

    [Theory]
    [InlineData("http://evil.example/path")]
    [InlineData("https://evil.example/path")]
    [InlineData("//evil.example/path")]
    public void Register_WithInvalidReturnUrl_UsesDefaultRedirect(string returnUrl)
    {
        var controller = BuildController(returnUrl, "trust-123");

        var result = controller.Register();

        var challenge = Assert.IsType<ChallengeResult>(result);
        Assert.Equal("/default", challenge.Properties?.RedirectUri);
        Assert.Equal("create", challenge.Properties?.Parameters["prompt"]);
    }

    [Fact]
    public void Register_WithoutReturnUrl_UsesDefaultRedirect()
    {
        var controller = BuildController(null, "trust-123");

        var result = controller.Register();

        var challenge = Assert.IsType<ChallengeResult>(result);
        Assert.Equal("/default", challenge.Properties?.RedirectUri);
        Assert.Equal("create", challenge.Properties?.Parameters["prompt"]);
    }

    private static OidcController BuildController(string? returnUrl = null, string? trustId = null)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:OIDC:PostLoginRedirect"] = "/default",
            })
            .Build();

        var controller = new OidcController(
            config,
            new StubTokenStore(),
            new StubHttpClientFactory(),
            new StubHostEnvironment(),
            new StubTrustsApiClient());

        var queryValues = new List<string>();
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            queryValues.Add($"returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        if (!string.IsNullOrWhiteSpace(trustId))
        {
            queryValues.Add($"trustId={Uri.EscapeDataString(trustId)}");
        }

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = queryValues.Count > 0
            ? new QueryString($"?{string.Join("&", queryValues)}")
            : QueryString.Empty;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
        };

        return controller;
    }

    private sealed class StubTrustsApiClient : ITrustsApiClient
    {
        public Task<CreateTrustResponse> CreateTrustAsync(CreateTrustRequest request, CancellationToken ct = default)
            => throw new NotSupportedException();
    }

    private sealed class StubTokenStore : ITokenStore
    {
        public Task SaveTokensAsync(string sessionId, string userId, string? trustId, string? accessToken, string? refreshToken, string? idToken, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<(string userId, string? trustId, string? accessToken, string? refreshToken, string? idToken)?> TryGetTokensAsync(string sessionId, CancellationToken cancellationToken = default)
            => Task.FromResult<(string userId, string? trustId, string? accessToken, string? refreshToken, string? idToken)?>(null);

        public Task RemoveAsync(string sessionId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
            => new();
    }

    private sealed class StubHostEnvironment : Microsoft.Extensions.Hosting.IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";

        public string ApplicationName { get; set; } = "CommonGround.Web.Gateway.Tests";

        public string ContentRootPath { get; set; } = "/";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
