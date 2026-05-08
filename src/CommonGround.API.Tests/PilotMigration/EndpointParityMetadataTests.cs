using System.Reflection;
using CommonGround.API.Identity;
using CommonGround.API.TrustManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace CommonGround.API.Tests.PilotMigration;

public sealed class EndpointParityMetadataTests
{
    [Fact]
    public void UsersEnsure_WolverineRoute_MatchesControllerRoute()
    {
        var method = typeof(UsersEndpoints).GetMethod(nameof(UsersEndpoints.EnsureUser), BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(method);
        var routeTemplate = GetWolverinePostTemplate(method!);

        Assert.Equal("/api/users/ensure", routeTemplate);
    }

    [Fact]
    public void UsersEnsure_WolverineEndpoint_DoesNotRequireHttpContextClaimRouting()
    {
        var method = typeof(UsersEndpoints).GetMethod(nameof(UsersEndpoints.EnsureUser), BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(method);
        var hasHttpContextParameter = method!.GetParameters().Any(p => p.ParameterType == typeof(HttpContext));

        Assert.False(hasHttpContextParameter);
    }

    [Fact]
    public void TrustsCreate_WolverineRoute_MatchesExpectedRoute()
    {
        var method = typeof(TrustsEndpoints).GetMethod(nameof(TrustsEndpoints.CreateTrust), BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(method);
        var routeTemplate = GetWolverinePostTemplate(method!);

        Assert.Equal("/api/trusts", routeTemplate);
    }

    [Fact]
    public void TrustsCreate_WolverineEndpoint_RequiresAuthenticationWithoutPolicy()
    {
        var method = typeof(TrustsEndpoints).GetMethod(nameof(TrustsEndpoints.CreateTrust), BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(method);
        var authorize = method!.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        Assert.Null(authorize!.Policy);
    }

    private static string GetWolverinePostTemplate(MemberInfo method)
    {
        var attributeData = method.GetCustomAttributesData()
            .First(a => string.Equals(a.AttributeType.Name, "WolverinePostAttribute", StringComparison.Ordinal));

        return (string)attributeData.ConstructorArguments[0].Value!;
    }
}
