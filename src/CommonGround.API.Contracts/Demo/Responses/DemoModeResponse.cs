namespace CommonGround.API.Contracts.Demo.Responses;

/// <summary>What a signed-out visitor to a demo server is told before signing up.</summary>
/// <param name="SandboxDays">How many days a sample trust lives before it is deleted.</param>
public sealed record DemoModeResponse(int SandboxDays);
