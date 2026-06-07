using System.Security.Claims;
using CommonGround.Management.Gateway.Configuration;
using CommonGround.Management.Gateway.Controllers;
using CommonGround.Management.Gateway.Models;
using CommonGround.Management.Gateway.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace CommonGround.API.Tests.Gateway;

public sealed class AuthControllerCurrentUserTests
{
    [Fact]
    public async Task GetCurrentUser_WithTrustInSession_ReturnsTrustFields()
    {
        var tokenStore = new StubTokenStore
        {
            Tokens = ("user-123", "trust-456", "access", "refresh", "id"),
        };

        var controller = BuildController(
            sessionId: "session-with-trust",
            preferredUsername: "alice",
            email: "alice@example.com");

        var result = await controller.GetCurrentUser(tokenStore);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<CurrentUserDto>(ok.Value);

        Assert.Equal("user-123", dto.Id);
        Assert.Equal("alice", dto.Username);
        Assert.Equal("alice@example.com", dto.Email);
        Assert.Equal("trust-456", dto.TrustId);
        Assert.True(dto.HasTrust);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutTrustInSession_ReturnsTrustlessFields()
    {
        var tokenStore = new StubTokenStore
        {
            Tokens = ("user-789", null, "access", "refresh", "id"),
        };

        var controller = BuildController(
            sessionId: "session-no-trust",
            preferredUsername: "bob",
            email: "bob@example.com");

        var result = await controller.GetCurrentUser(tokenStore);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<CurrentUserDto>(ok.Value);

        Assert.Equal("user-789", dto.Id);
        Assert.Equal("bob", dto.Username);
        Assert.Equal("bob@example.com", dto.Email);
        Assert.Null(dto.TrustId);
        Assert.False(dto.HasTrust);
    }

    private static AuthController BuildController(string sessionId, string preferredUsername, string email)
    {
        var controller = new AuthController(new StubHostEnvironment());
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Cookie = $"{GatewayDefaults.Cookies.SessionCookie}={sessionId}";
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("preferred_username", preferredUsername),
            new Claim("email", email),
        ],
        authenticationType: "Test"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
        };

        return controller;
    }

    private sealed class StubTokenStore : ITokenStore
    {
        public (string userId, string? trustId, string? accessToken, string? refreshToken, string? idToken)? Tokens { get; init; }

        public Task SaveTokensAsync(string sessionId, string userId, string? trustId, string? accessToken, string? refreshToken, string? idToken, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<(string userId, string? trustId, string? accessToken, string? refreshToken, string? idToken)?> TryGetTokensAsync(string sessionId, CancellationToken cancellationToken = default)
            => Task.FromResult(Tokens);

        public Task RemoveAsync(string sessionId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class StubHostEnvironment : Microsoft.Extensions.Hosting.IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";

        public string ApplicationName { get; set; } = "CommonGround.Management.Gateway.Tests";

        public string ContentRootPath { get; set; } = "/";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
