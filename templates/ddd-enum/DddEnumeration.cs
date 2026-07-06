using CommonGround.SharedKernel.Domain;

namespace DddNamespace;

/// <summary>TODO: Describe the DddEnumeration enumeration.</summary>
public sealed class DddEnumeration : Enumeration<DddEnumeration>
{
    /// <summary>TODO: Replace with real values.</summary>
    public static readonly DddEnumeration Default = new(1, "Default");

    private DddEnumeration(int value, string displayName) : base(value, displayName) { }
}
