using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>How a notice reached the homeowner.</summary>
public sealed class NoticeDelivery : Enumeration<NoticeDelivery>
{
    /// <summary>Ordinary post.</summary>
    public static readonly NoticeDelivery FirstClassMail = new(1, "FirstClassMail");

    /// <summary>Certified mail, return receipt requested.</summary>
    public static readonly NoticeDelivery CertifiedMail = new(2, "CertifiedMail");

    /// <summary>Registered mail, return receipt requested.</summary>
    public static readonly NoticeDelivery RegisteredMail = new(3, "RegisteredMail");

    /// <summary>Handed to the homeowner in person.</summary>
    public static readonly NoticeDelivery HandDelivered = new(4, "HandDelivered");

    /// <summary>Sent by email.</summary>
    public static readonly NoticeDelivery Email = new(5, "Email");

    /// <summary>
    /// Gets whether the delivery meets the lease's notice clause (Art 14.2): handed over in person, or sent by certified
    /// or registered mail, return receipt requested. A notice of default must be delivered this way.
    /// </summary>
    public bool MeetsNoticeClause => this == CertifiedMail || this == RegisteredMail || this == HandDelivered;

    private NoticeDelivery(int value, string displayName) : base(value, displayName) { }
}
