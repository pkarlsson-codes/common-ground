using System.Net.Http.Headers;
using CommonGround.Homeowner.Gateway.Configuration;

namespace CommonGround.Homeowner.Gateway.Services;

/// <summary>
/// Delegating handler that attaches a short-lived internal JWT to calls to the Core Platform API.
/// </summary>
public sealed partial class InternalGatewayAuthHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IInternalJwtService _jwtService;
    private readonly ITokenStore _tokenStore;
    private readonly ILogger<InternalGatewayAuthHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalGatewayAuthHandler"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for current HTTP context.</param>
    /// <param name="jwtService">Internal JWT minting service.</param>
    /// <param name="tokenStore">Token store for resolving session tokens.</param>
    /// <param name="logger">Logger instance.</param>
    public InternalGatewayAuthHandler(
        IHttpContextAccessor httpContextAccessor,
        IInternalJwtService jwtService,
        ITokenStore tokenStore,
        ILogger<InternalGatewayAuthHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _tokenStore = tokenStore;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        string? userId = null;
        string? trustId = null;
        string? sessionId = null;

        if (httpContext != null)
        {
            var requestPath = httpContext.Request.Path.Value ?? string.Empty;

            userId = httpContext.User.FindFirst("user_id")?.Value;

            sessionId = httpContext.Request.Cookies[GatewayDefaults.Cookies.SessionCookie];

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                var stored = await _tokenStore.TryGetTokensAsync(sessionId, cancellationToken).ConfigureAwait(false);
                if (stored.HasValue)
                {
                    userId ??= stored.Value.userId;
                    trustId = stored.Value.trustId;
                    Logs.ResolvedFromTokenStore(_logger, stored.Value.userId);
                }
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                Logs.AnonymousRequest(_logger, requestPath);
            }
            else
            {
                Logs.ForwardingWithUser(_logger, userId, requestPath);
            }
        }

        var token = _jwtService.GenerateToken(userId, trustId, sessionId);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static partial class Logs
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Resolved userId {UserId} from token store for session (claims principal had no userId claim)")]
        internal static partial void ResolvedFromTokenStore(ILogger logger, string userId);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "No userId resolved from claims or token store; downstream request will be treated as anonymous. Path: {Path}")]
        internal static partial void AnonymousRequest(ILogger logger, string path);

        [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Forwarding request with userId {UserId}. Path: {Path}")]
        internal static partial void ForwardingWithUser(ILogger logger, string userId, string path);
    }
}
