using CommonGround.Homeowner.Gateway.Configuration;

namespace CommonGround.Homeowner.Gateway.Services;

/// <summary>
/// Delegating handler that attaches a short-lived internal service JWT to outbound requests.
/// </summary>
public sealed class InternalServiceTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IInternalJwtService _jwtService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalServiceTokenHandler"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="jwtService">The internal JWT service.</param>
    public InternalServiceTokenHandler(IHttpContextAccessor httpContextAccessor, IInternalJwtService jwtService)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sessionId = _httpContextAccessor.HttpContext?.Request.Cookies[GatewayDefaults.Cookies.SessionCookie];
        var token = _jwtService.GenerateServiceToken(sessionId);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
