using System.Globalization;
using CommonGround.API.Infrastructure.ExceptionHandling;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.PropertyRegistry.Application.Commands;
using CommonGround.PropertyRegistry.Application.Queries;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;

namespace CommonGround.API.PortfolioImport;

/// <summary>One problem with an import file.</summary>
/// <param name="row">The spreadsheet row number; the header is row 1.</param>
/// <param name="column">The column header, or null when the problem is with the row as a whole.</param>
/// <param name="message">What is wrong, in words staff can act on.</param>
internal sealed class PortfolioImportError(int row, string? column, string message)
{
    /// <summary>Gets the spreadsheet row number.</summary>
    public int Row { get; } = row;

    /// <summary>Gets the column header, or null.</summary>
    public string? Column { get; } = column;

    /// <summary>Gets what is wrong.</summary>
    public string Message { get; } = message;
}

/// <summary>A command the import sends, and the row it came from.</summary>
/// <param name="row">The spreadsheet row number.</param>
/// <param name="command">The command.</param>
internal sealed class PortfolioImportStep(int row, object command)
{
    /// <summary>Gets the spreadsheet row number.</summary>
    public int Row { get; } = row;

    /// <summary>Gets the command.</summary>
    public object Command { get; } = command;
}

/// <summary>What an import file would create, or why it can't be imported.</summary>
internal sealed class PortfolioImportPlan
{
    /// <summary>Gets every problem found. The file is imported only when this is empty.</summary>
    public List<PortfolioImportError> Errors { get; } = [];

    /// <summary>Gets the commands to send, in row order: each row's parcel, then home, then lease.</summary>
    public List<PortfolioImportStep> Steps { get; } = [];

    /// <summary>Gets or sets the number of land parcels to register.</summary>
    public int ParcelsToCreate { get; set; }

    /// <summary>Gets or sets the number of distinct land parcels that already exist.</summary>
    public int ParcelsExisting { get; set; }

    /// <summary>Gets or sets the number of homes to register.</summary>
    public int HomesToCreate { get; set; }

    /// <summary>Gets or sets the number of homes that already exist.</summary>
    public int HomesExisting { get; set; }

    /// <summary>Gets or sets the number of ground leases to create.</summary>
    public int LeasesToCreate { get; set; }

    /// <summary>Gets or sets the number of rows whose home already has an active lease.</summary>
    public int LeasesExisting { get; set; }
}

/// <summary>Checks an import file row by row against what the trust already has, and plans the commands.</summary>
/// <remarks>
/// Each new parcel, home and lease is run through its domain factory, so the check finds every rule the commands
/// would break without sending them.
/// </remarks>
internal static class PortfolioImportPlanner
{
    /// <summary>Plans the import of a file.</summary>
    /// <param name="table">The file.</param>
    /// <param name="properties">The trust's existing parcels and homes.</param>
    /// <param name="leasing">The trust's active templates and leased homes.</param>
    /// <param name="today">Today in the trust's time zone.</param>
    /// <param name="importedBy">The staff member importing.</param>
    /// <returns>The errors, or the commands to send.</returns>
    public static PortfolioImportPlan Plan(
        CsvTable table,
        RegisteredProperties properties,
        LeasingForImport leasing,
        DateOnly today,
        UserId importedBy)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(properties);
        ArgumentNullException.ThrowIfNull(leasing);

        var plan = new PortfolioImportPlan();

        var missing = PortfolioColumns.Required.Where(c => !table.HasColumn(c)).ToList();
        foreach (var column in missing)
        {
            plan.Errors.Add(new PortfolioImportError(1, column, "This column is missing. Start from the template."));
        }

        if (missing.Count > 0)
        {
            return plan;
        }

        if (table.Rows.Count == 0)
        {
            plan.Errors.Add(new PortfolioImportError(1, null, "The file has no rows after the header."));
            return plan;
        }

        var context = new PlanningContext(properties, leasing, today, importedBy);
        foreach (var row in table.Rows)
        {
            PlanRow(new RowReader(table, row, plan.Errors), context, plan);
        }

        if (plan.Errors.Count > 0)
        {
            plan.Steps.Clear();
        }

        return plan;
    }

    // Reads every cell of the row even after an earlier part fails, so the check lists all of the row's errors.
    private static void PlanRow(RowReader row, PlanningContext context, PortfolioImportPlan plan)
    {
        var parcelId = PlanParcel(row, context, plan);
        var home = PlanHome(row, parcelId, context, plan);
        PlanLease(row, parcelId, home, context, plan);
    }

    private static string? PlanParcel(RowReader row, PlanningContext context, PortfolioImportPlan plan)
    {
        var number = row.Text(PortfolioColumns.ParcelNumber, required: true);
        if (number is null)
        {
            return null;
        }

        var key = number.ToUpperInvariant();
        if (context.ParcelsByNumber.TryGetValue(key, out var parcelId))
        {
            if (context.ExistingParcelsCounted.Add(key))
            {
                plan.ParcelsExisting++;
            }

            return parcelId;
        }

        if (context.NewParcelsByNumber.TryGetValue(key, out var plannedId))
        {
            return plannedId;
        }

        var address = row.Text(PortfolioColumns.ParcelAddress, required: true);
        var acres = row.Decimal(PortfolioColumns.ParcelAcres, required: true);
        var acquiredOn = row.Date(PortfolioColumns.AcquiredOn, required: true);
        var acquiredBy = row.Choice(PortfolioColumns.AcquiredBy, AcquisitionMethod.GetAll(), required: true);
        var covenant = row.Text(PortfolioColumns.ParcelCovenant, required: false);
        if (address is null || acres is null || acquiredOn is null || acquiredBy is null)
        {
            return null;
        }

        var id = LandParcelId.Generate();
        var command = new RegisterLandParcel(id, number, address, acres.Value, acquiredOn.Value, acquiredBy, covenant, context.ImportedBy);
        if (!row.Check(() => LandParcel.Register(id, number, address, acres.Value, acquiredOn.Value, acquiredBy, covenant, context.ImportedBy)))
        {
            return null;
        }

        context.NewParcelsByNumber[key] = id.Value;
        plan.Steps.Add(new PortfolioImportStep(row.Line, command));
        plan.ParcelsToCreate++;
        return id.Value;
    }

    private static PlannedHome? PlanHome(RowReader row, string? parcelId, PlanningContext context, PortfolioImportPlan plan)
    {
        var unit = row.Text(PortfolioColumns.Unit, required: false);
        if (parcelId is not null)
        {
            var key = (parcelId, (unit ?? string.Empty).ToUpperInvariant());

            if (context.HomesByKey.TryGetValue(key, out var existingId))
            {
                plan.HomesExisting++;
                return new PlannedHome(existingId, isNew: false);
            }

            if (context.NewHomeRows.TryGetValue(key, out var firstRow))
            {
                row.Error(null, $"This is the same home as row {firstRow}: same Parcel Number and Unit. Give each home on a parcel its own Unit.");
                return null;
            }

            context.NewHomeRows[key] = row.Line;
        }

        var homeType = row.Choice(PortfolioColumns.HomeType, HomeType.GetAll(), required: true);
        var bedrooms = row.Integer(PortfolioColumns.Bedrooms, required: true);
        var bathrooms = row.Decimal(PortfolioColumns.Bathrooms, required: true);
        var accessible = row.YesNo(PortfolioColumns.Accessible);
        var incomeLimit = row.IncomeLimit(PortfolioColumns.IncomeLimit);
        var squareFeet = row.Decimal(PortfolioColumns.SquareFeet, required: true);
        var yearBuilt = row.Integer(PortfolioColumns.YearBuilt, required: true);
        var assessedValue = row.Decimal(PortfolioColumns.AssessedValue, required: true);
        var assessedOn = row.Date(PortfolioColumns.AssessedOn, required: true);
        if (parcelId is null || homeType is null || bedrooms is null || bathrooms is null || accessible is null
            || incomeLimit is null || squareFeet is null || yearBuilt is null || assessedValue is null || assessedOn is null)
        {
            return null;
        }

        var id = HomeId.Generate();
        var landParcelId = LandParcelId.Create(parcelId);
        var command = new RegisterHome(
            id, landParcelId, unit, homeType, bedrooms.Value, bathrooms.Value, accessible.Value, incomeLimit.PercentOfAmi, squareFeet.Value,
            yearBuilt.Value, assessedValue.Value, assessedOn.Value, AppraiserName: null, context.ImportedBy);

        var valid = row.Check(() =>
        {
            var assessment = UnitAssessment.Create(
                assessedValue.Value, assessedOn.Value, appraiserName: null, conditionNotes: null, context.ImportedBy,
                TimeProviderContext.Current.UtcNow, context.Today);
            Home.Register(
                id, landParcelId, unit, homeType, bedrooms.Value, bathrooms.Value, accessible.Value, incomeLimit.PercentOfAmi, squareFeet.Value,
                yearBuilt.Value, assessment, context.ImportedBy);
        });
        if (!valid)
        {
            return null;
        }

        plan.Steps.Add(new PortfolioImportStep(row.Line, command));
        plan.HomesToCreate++;
        return new PlannedHome(id.Value, isNew: true);
    }

    private static void PlanLease(RowReader row, string? parcelId, PlannedHome? plannedHome, PlanningContext context, PortfolioImportPlan plan)
    {
        var name = row.Text(PortfolioColumns.HomeownerName, required: false);
        if (name is null)
        {
            return;
        }

        if (plannedHome is { IsNew: false } existing && context.LeasedHomeIds.Contains(existing.Id))
        {
            plan.LeasesExisting++;
            return;
        }

        var email = row.Text(PortfolioColumns.HomeownerEmail, required: true);
        var phone = row.Text(PortfolioColumns.HomeownerPhone, required: false);
        var template = row.Template(PortfolioColumns.LeaseTemplate, context.TemplatesByName);
        var leaseStart = row.Date(PortfolioColumns.LeaseStart, required: true);
        var basePrice = row.Decimal(PortfolioColumns.BasePrice, required: true);
        var appraisedValue = row.Decimal(PortfolioColumns.AppraisedValue, required: true);
        var landUseFee = row.Decimal(PortfolioColumns.LandUseFee, required: false);
        var repairReserveFee = row.Decimal(PortfolioColumns.RepairReserveFee, required: false);

        // Both are required: a guess would hide what the homeowner already owes, or charge years of fees.
        var trackedFrom = row.Date(PortfolioColumns.FeesTrackedFrom, required: true);
        var amountOwed = row.Decimal(PortfolioColumns.AmountOwed, required: true);
        if (parcelId is null || plannedHome is not { } home || email is null || template is null || leaseStart is null || basePrice is null || appraisedValue is null
            || trackedFrom is null || amountOwed is null
            || row.HasErrorIn(PortfolioColumns.LandUseFee, PortfolioColumns.RepairReserveFee, PortfolioColumns.FeesTrackedFrom, PortfolioColumns.AmountOwed))
        {
            return;
        }

        var id = GroundLeaseId.Generate();

        RegisterExistingGroundLease? command = null;
        var valid = row.Check(() =>
        {
            var homeowner = Homeowner.From(name, email, phone);
            var feeTerms = LeaseFeeTerms.Create(
                Money.Create(landUseFee ?? template.LandUseFee),
                Money.Create(repairReserveFee ?? template.RepairReserveFee),
                template.LateInterestRate,
                template.InterestWaiverDays);
            var resaleTerms = ResalePolicyTerms.FromString(
                template.FormulaKind, template.AppreciationShare, template.IndexType, template.BaselineIndexValue,
                template.AnnualPercentage, template.ImprovementsCap);
            var debtCap = MortgageDebtCap.Create(template.MaxMortgageDebtPercent);
            var tracking = LeaseFeeTracking.Create(trackedFrom.Value, Money.Create(amountOwed.Value), leaseStart.Value);

            GroundLease.RegisterExisting(
                id, parcelId, home.Id, homeowner, leaseStart.Value, template.TermYears, template.OptionWindowDays, feeTerms, debtCap,
                Money.Create(basePrice.Value), Money.Create(appraisedValue.Value), template.CovenantTerms, resaleTerms, HomeCommitments.None,
                context.ImportedBy, tracking);

            command = new RegisterExistingGroundLease(
                id, parcelId, home.Id, homeowner, leaseStart.Value, template.TermYears, template.OptionWindowDays, feeTerms, debtCap,
                Money.Create(basePrice.Value), Money.Create(appraisedValue.Value), template.CovenantTerms, resaleTerms, context.ImportedBy, tracking);
        });
        if (!valid || command is null)
        {
            return;
        }

        plan.Steps.Add(new PortfolioImportStep(row.Line, command));
        plan.LeasesToCreate++;
    }

    private readonly struct PlannedHome(string id, bool isNew)
    {
        public string Id { get; } = id;

        public bool IsNew { get; } = isNew;
    }

    /// <summary>A home's income limit read from the file: a percent of AMI, or null for none.</summary>
    private sealed class ImportedIncomeLimit(int? percentOfAmi)
    {
        public int? PercentOfAmi { get; } = percentOfAmi;
    }

    private sealed class PlanningContext
    {
        public PlanningContext(RegisteredProperties properties, LeasingForImport leasing, DateOnly today, UserId importedBy)
        {
            Today = today;
            ImportedBy = importedBy;
            ParcelsByNumber = properties.Parcels
                .GroupBy(p => p.LegalParcelNumber.Trim().ToUpperInvariant())
                .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.Ordinal);
            HomesByKey = properties.Homes
                .GroupBy(h => (h.LandParcelId, (h.UnitDesignation ?? string.Empty).Trim().ToUpperInvariant()))
                .ToDictionary(g => g.Key, g => g.First().Id);
            TemplatesByName = leasing.ActiveTemplates
                .GroupBy(t => t.TemplateName.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            LeasedHomeIds = [.. leasing.LeasedHomeIds];
        }

        public DateOnly Today { get; }

        public UserId ImportedBy { get; }

        public Dictionary<string, string> ParcelsByNumber { get; }

        public Dictionary<(string ParcelId, string Unit), string> HomesByKey { get; }

        public Dictionary<string, GroundLeaseTemplateDetailView> TemplatesByName { get; }

        public HashSet<string> LeasedHomeIds { get; }

        public HashSet<string> ExistingParcelsCounted { get; } = new(StringComparer.Ordinal);

        public Dictionary<string, string> NewParcelsByNumber { get; } = new(StringComparer.Ordinal);

        public Dictionary<(string ParcelId, string Unit), int> NewHomeRows { get; } = [];
    }

    /// <summary>Reads one row's cells into typed values, recording an error for each cell it can't read.</summary>
    private sealed class RowReader(CsvTable table, CsvRow row, List<PortfolioImportError> errors)
    {
        private static readonly string[] DateFormats = ["yyyy-MM-dd", "M/d/yyyy", "M/d/yy"];

        public int Line => row.Line;

        public void Error(string? column, string message) => errors.Add(new PortfolioImportError(row.Line, column, message));

        public bool HasErrorIn(params string[] columns) =>
            errors.Exists(e => e.Row == row.Line && e.Column is not null && columns.Contains(e.Column));

        public string? Text(string column, bool required)
        {
            var value = table.Value(row, column);
            if (value.Length > 0)
            {
                return value;
            }

            if (required)
            {
                Error(column, "This is required.");
            }

            return null;
        }

        public decimal? Decimal(string column, bool required)
        {
            var text = Text(column, required);
            if (text is null)
            {
                return null;
            }

            var cleaned = text.Replace("$", string.Empty, StringComparison.Ordinal).Replace(",", string.Empty, StringComparison.Ordinal).Trim();
            if (decimal.TryParse(cleaned, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            Error(column, $"\"{text}\" is not a number.");
            return null;
        }

        public int? Integer(string column, bool required)
        {
            var text = Text(column, required);
            if (text is null)
            {
                return null;
            }

            if (int.TryParse(text.Replace(",", string.Empty, StringComparison.Ordinal), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            Error(column, $"\"{text}\" is not a whole number.");
            return null;
        }

        public DateOnly? Date(string column, bool required)
        {
            var text = Text(column, required);
            if (text is null)
            {
                return null;
            }

            if (DateOnly.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
            {
                return value;
            }

            Error(column, $"\"{text}\" is not a date. Write it as 2005-06-01 or 6/1/2005.");
            return null;
        }

        public bool? YesNo(string column)
        {
            var text = Text(column, required: false);
            switch (text?.ToUpperInvariant())
            {
                case null or "NO" or "N" or "FALSE":
                    return false;
                case "YES" or "Y" or "TRUE":
                    return true;
                default:
                    Error(column, $"\"{text}\" is not Yes or No.");
                    return null;
            }
        }

        public ImportedIncomeLimit? IncomeLimit(string column)
        {
            var text = Text(column, required: false);
            if (text is null)
            {
                return new ImportedIncomeLimit(Home.DefaultIncomeLimitPercentOfAmi);
            }

            if (string.Equals(text, "None", StringComparison.OrdinalIgnoreCase))
            {
                return new ImportedIncomeLimit(null);
            }

            if (int.TryParse(text.TrimEnd('%').Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var percent))
            {
                return new ImportedIncomeLimit(percent);
            }

            Error(column, $"\"{text}\" is not a percent of AMI, such as 80, or None.");
            return null;
        }

        public T? Choice<T>(string column, IEnumerable<T> options, bool required)
            where T : Enumeration<T>
        {
            var text = Text(column, required);
            if (text is null)
            {
                return null;
            }

            var all = options.ToList();
            var match = all.Find(o => string.Equals(Squash(o.DisplayName), Squash(text), StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                return match;
            }

            Error(column, $"\"{text}\" is not one of: {string.Join(", ", all.Select(o => o.DisplayName))}.");
            return null;
        }

        public GroundLeaseTemplateDetailView? Template(string column, Dictionary<string, GroundLeaseTemplateDetailView> templates)
        {
            var text = Text(column, required: true);
            if (text is null)
            {
                return null;
            }

            if (templates.TryGetValue(text, out var template))
            {
                return template;
            }

            var names = string.Join(", ", templates.Keys.Order(StringComparer.OrdinalIgnoreCase));
            var message = templates.Count == 0
                ? "The trust has no active ground lease templates. Create one before importing leases."
                : $"No active ground lease template is named \"{text}\". Templates: {names}.";
            Error(column, message);
            return null;
        }

        /// <summary>Runs a domain factory and records the rule it breaks, if any, on the column that breaks it.</summary>
        public bool Check(Action create)
        {
            try
            {
                create();
                return true;
            }
            catch (DomainException ex)
            {
                var column = ex.Code is { } code ? PortfolioColumns.ByErrorCode.GetValueOrDefault(code) : null;
                Error(column, DomainExceptionHandler.GetUserFacingMessage(ex));
                return false;
            }
        }

        private static string Squash(string value) =>
            value.Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal);
    }
}
