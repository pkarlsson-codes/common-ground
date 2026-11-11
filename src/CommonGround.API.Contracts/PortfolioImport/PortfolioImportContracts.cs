namespace CommonGround.API.Contracts.PortfolioImport;

/// <summary>A portfolio import file: one CSV row per home, with its land parcel and its ground lease.</summary>
/// <param name="Csv">The file's text.</param>
public sealed record PortfolioImportRequest(string Csv);

/// <summary>One problem with an import file.</summary>
/// <param name="Row">The spreadsheet row number; the header is row 1.</param>
/// <param name="Column">The column header, or null when the problem is with the row as a whole.</param>
/// <param name="Message">What is wrong.</param>
public sealed record PortfolioImportErrorDto(int Row, string? Column, string Message);

/// <summary>The result of checking or importing a file.</summary>
/// <remarks>
/// When <c>Errors</c> is not empty nothing was imported. The counts say what the import creates (or created) and
/// what already existed and is skipped. <c>StoppedAtRow</c> is set when an import failed part way; everything
/// before that row was created, and importing the same file again finishes it.
/// </remarks>
public sealed record PortfolioImportResponse(
    IReadOnlyList<PortfolioImportErrorDto> Errors,
    int ParcelsToCreate,
    int ParcelsExisting,
    int HomesToCreate,
    int HomesExisting,
    int LeasesToCreate,
    int LeasesExisting,
    bool Imported,
    int? StoppedAtRow = null);
