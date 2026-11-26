using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Tests.Domain.HomeSelections;

/// <summary>A waiting list of three ready households and an income limit schedule, for comparing and choosing.</summary>
public class SelectionTestBed
{
    public UserId StaffUser { get; } = UserId.Generate();

    public string HomeId { get; } = "home-1";

    public DateOnly Today { get; } = new(2026, 11, 12);

    public IReadOnlyList<WaitingApplication> Waiting { get; } =
    [
        Ready("application-a", 48000m, 3, true, new DateOnly(2026, 3, 1)),
        Ready("application-b", 62000m, 2, false, new DateOnly(2026, 4, 1)),
        Ready("application-c", 55000m, 4, false, new DateOnly(2026, 5, 1)),
    ];

    public HomeownerApplicationId A => Waiting[0].ApplicationId;
    public HomeownerApplicationId B => Waiting[1].ApplicationId;
    public HomeownerApplicationId C => Waiting[2].ApplicationId;

    /// <summary>A schedule with an 80% level whose 2-person limit (57,000) is below application B's income.</summary>
    public IncomeLimitTable Limits { get; } = IncomeLimitTable.Create(
        new DateOnly(2026, 4, 1),
        [IncomeLimitLevel.Create(80, [.. new[] { 50000m, 57000m, 64000m, 71000m, 77000m, 82000m, 88000m, 94000m }.Select(Money.Create)])]);

    public SelectionTerms Terms(DateOnly? selectedDate = null) => new("template-1", 200000m, selectedDate ?? Today);

    public WaitingListComparison Compare(
        IReadOnlyList<WaitingApplication>? waiting = null,
        int? incomeLimitPercentOfAmi = null,
        IncomeLimitTable? limits = null,
        DateOnly? asOf = null) =>
        WaitingListComparison.Create(waiting ?? Waiting, incomeLimitPercentOfAmi, limits, asOf ?? Today);

    public WaitingListComparison CompareWithIncomeLimit() => Compare(incomeLimitPercentOfAmi: 80, limits: Limits);

    public static WaitingApplication Ready(
        string applicationId, decimal annualIncome, int householdSize, bool requiresAccessibility, DateOnly waitingSince) =>
        new(
            HomeownerApplicationId.Create(applicationId),
            HouseholdId.Generate(),
            Money.Create(annualIncome),
            householdSize,
            requiresAccessibility,
            waitingSince,
            HasPrimaryMember: true,
            IsHousingCounselingCompleted: true,
            IsCltOrientationCompleted: true,
            PrequalificationExpiryDate: new DateOnly(2027, 6, 1));
}
