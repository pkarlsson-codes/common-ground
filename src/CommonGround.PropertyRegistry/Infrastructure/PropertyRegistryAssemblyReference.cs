namespace CommonGround.PropertyRegistry.Infrastructure;

/// <summary>
/// Assembly reference type for the PropertyRegistry module, used for assembly scanning and type discovery.
/// </summary>
public sealed class PropertyRegistryAssemblyReference
{
    /// <summary>
    /// Gets the assembly containing the PropertyRegistry module, used for assembly scanning and type discovery.
    /// </summary>
    public static readonly System.Reflection.Assembly Assembly =
        typeof(PropertyRegistryAssemblyReference).Assembly;
}