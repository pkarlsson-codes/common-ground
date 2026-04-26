using CommonGround.SharedKernel.Domain;

namespace CommonGround.Documents.Domain.Documents;

/// <summary>Retention class — drives purge / legal-hold rules (deferred).</summary>
public sealed class RetentionClass : Enumeration<RetentionClass>
{
    /// <summary>Permanent — never purged.</summary>
    public static readonly RetentionClass Permanent = new(1, "Permanent");

    /// <summary>Long-term — kept for 7 years.</summary>
    public static readonly RetentionClass LongTerm7yr = new(2, "LongTerm7yr");

    /// <summary>Standard — kept for 3 years.</summary>
    public static readonly RetentionClass Standard3yr = new(3, "Standard3yr");

    /// <summary>Transient — eligible for early purge.</summary>
    public static readonly RetentionClass Transient = new(4, "Transient");

    private RetentionClass(int value, string displayName) : base(value, displayName) { }
}
