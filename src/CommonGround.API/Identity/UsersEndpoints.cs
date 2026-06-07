using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.Identity.Application.Commands;
using CommonGround.SharedKernel.Domain;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Identity;

public static class UsersEndpoints
{
    [WolverinePost("/api/users/ensure")]
    [NotTenanted]
    public static async Task<IResult> EnsureUser(
        EnsureUserRequest request,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await bus.InvokeAsync<EnsureUserResult>(
            new EnsureUser(request.KeycloakSubject, request.Email, request.DisplayName),
            cancellationToken).ConfigureAwait(false);

        return Results.Ok(new EnsureUserResponse(result.UserId, result.TrustId));
    }
}