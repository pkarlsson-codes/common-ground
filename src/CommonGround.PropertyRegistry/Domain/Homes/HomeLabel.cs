namespace CommonGround.PropertyRegistry.Domain.Homes;

/// <summary>Names a home the way staff refer to it: its parcel's address, plus its unit designation when it has one.</summary>
public static class HomeLabel
{
    /// <summary>Formats a label such as "14 Alder Lane" or "14 Alder Lane, Unit B".</summary>
    public static string Format(string parcelAddress, string? unitDesignation) =>
        string.IsNullOrWhiteSpace(unitDesignation) ? parcelAddress : $"{parcelAddress}, {unitDesignation}";
}
