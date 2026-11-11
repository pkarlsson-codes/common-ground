using System.Reflection;

namespace CommonGround.API.Infrastructure.Messaging;

/// <summary>
/// The rule for which messages leave the process: every type in the <c>.Events</c> namespace of a
/// <c>*.Contracts</c> assembly. A bounded context with a Contracts project publishes these for other
/// contexts, and for any service split out of the API later.
/// </summary>
internal static class IntegrationEvents
{
    /// <summary>Checks whether a type is an integration event.</summary>
    /// <param name="type">The message type.</param>
    /// <returns><c>true</c> when the type goes to the external transport.</returns>
    public static bool Includes(Type type) =>
        type.Namespace?.EndsWith(".Events", StringComparison.Ordinal) == true &&
        IsContractsAssembly(type.Assembly.GetName());

    private static bool IsContractsAssembly(AssemblyName name) =>
        name.Name?.EndsWith(".Contracts", StringComparison.Ordinal) == true;
}
