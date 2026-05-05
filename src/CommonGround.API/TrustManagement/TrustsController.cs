using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.TrustManagement.Application.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CommonGround.API.TrustManagement;

/// <summary>Manages CommonGround trust entities.</summary>
[ApiController]
[Route("api/trusts")]
public sealed class TrustsController(IMessageBus bus) : ControllerBase
{
    /// <summary>Creates a new trust.</summary>
    /// <param name="request">The request containing the trust name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stable ID of the newly created trust.</returns>
    [HttpPost]
    [Authorize(Policy = "ServiceToken")]
    public async Task<IActionResult> CreateTrust(
        [FromBody] CreateTrustRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await bus.InvokeAsync<CreateTrustResult>(
            new CreateTrust(request.Name),
            cancellationToken).ConfigureAwait(false);

        return Ok(new CreateTrustResponse(result.TrustId));
    }
}
