namespace CommonGround.Identity.Infrastructure;

/// <summary>Assembly reference type for the Identity module, used for assembly scanning and type discovery.</summary>
public sealed class IdentityAssemblyReference
{
    /// <summary>Gets the assembly containing the Identity module, used for assembly scanning and type discovery.</summary>
    public static readonly System.Reflection.Assembly Assembly =
        typeof(IdentityAssemblyReference).Assembly;
}
