namespace CommonGround.PropertyRegistry.Application.ReadModels;

public class LandParcelListItemView
{
    public string Id { get; set; } = string.Empty;

    public string LegalParcelNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public decimal AreaAcres { get; set; }

    public bool IsDecommissioned { get; set; }

    public DateTimeOffset RegisteredAt { get; set; }
}
