namespace CommonGround.PropertyRegistry.Application.ReadModels;

public class LandParcelView
{
    public string Id { get; set; } = string.Empty;

    public string LegalParcelNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public decimal AreaAcres { get; set; }

    public DateOnly AcquisitionDate { get; set; }

    public string AcquisitionMethod { get; set; } = string.Empty;

    public string? CovenantTerms { get; set; }

    public bool IsDecommissioned { get; set; }

    public DateTimeOffset RegisteredAt { get; set; }

    public string RegisteredBy { get; set; } = string.Empty;
}
