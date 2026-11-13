namespace CommonGround.API.Contracts.Demo.Responses;

/// <summary>A sample trust being filled, or ready.</summary>
/// <param name="TrustId">The sample trust's ID.</param>
/// <param name="Status"><c>Seeding</c> while it is filled, <c>Ready</c> when it can be used, <c>Failed</c> if filling it failed.</param>
public sealed record DemoSandboxResponse(string TrustId, string Status);

/// <summary>The signed-in user's latest sandbox.</summary>
/// <param name="Sandbox">The sandbox, or <see langword="null"/> when the user hasn't started one.</param>
public sealed record CurrentDemoSandboxResponse(DemoSandboxResponse? Sandbox);
