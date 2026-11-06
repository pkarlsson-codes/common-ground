using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.Resales;

/// <summary>How a homeowner's notice of intent to sell reached the trust.</summary>
public sealed class NoticeChannel : Enumeration<NoticeChannel>
{
    /// <summary>The homeowner phoned the trust.</summary>
    public static readonly NoticeChannel Phone = new(1, "Phone");

    /// <summary>The homeowner emailed the trust.</summary>
    public static readonly NoticeChannel Email = new(2, "Email");

    /// <summary>The homeowner wrote to the trust.</summary>
    public static readonly NoticeChannel Letter = new(3, "Letter");

    /// <summary>The homeowner told trust staff in person.</summary>
    public static readonly NoticeChannel InPerson = new(4, "InPerson");

    private NoticeChannel(int value, string displayName) : base(value, displayName) { }
}
