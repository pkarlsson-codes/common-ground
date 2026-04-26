using CommonGround.SharedKernel.Domain;

namespace CommonGround.Documents.Domain.Documents;

/// <summary>Sensitivity classification controlling read-access enforcement (deferred).</summary>
public sealed class SensitivityLevel : Enumeration<SensitivityLevel>
{
    /// <summary>Public — anyone with a CLT login may read.</summary>
    public static readonly SensitivityLevel Public = new(1, "Public");

    /// <summary>Internal — staff only (default).</summary>
    public static readonly SensitivityLevel Internal = new(2, "Internal");

    /// <summary>Confidential — staff with explicit need-to-know.</summary>
    public static readonly SensitivityLevel Confidential = new(3, "Confidential");

    /// <summary>Restricted — gated behind <c>pii:read</c> claim.</summary>
    public static readonly SensitivityLevel Restricted = new(4, "Restricted");

    private SensitivityLevel(int value, string displayName) : base(value, displayName) { }
}
