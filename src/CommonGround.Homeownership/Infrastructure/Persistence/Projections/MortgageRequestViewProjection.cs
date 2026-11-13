using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.MortgageRequests;
using CommonGround.Homeownership.Domain.MortgageRequests.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Homeownership.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="MortgageRequestView"/> from MortgageRequest events.</summary>
internal sealed class MortgageRequestViewProjection : SingleStreamProjection<MortgageRequestView, string>
{
    public static MortgageRequestView Create(MortgageRequestReceived e) =>
        new()
        {
            Id = e.AggregateId,
            GroundLeaseId = e.GroundLeaseId,
            HomeId = e.HomeId,
            HomeownerFullName = e.HomeownerFullName,
            Status = MortgageRequestStatus.Pending.DisplayName,
            ReceivedOn = e.ReceivedOn,
            Lender = e.Lender,
            Purpose = e.Purpose.DisplayName,
            Principal = e.Principal.Amount,
            TotalDebtAfterClosing = e.TotalDebtAfterClosing.Amount,
            InterestRatePercent = e.InterestRatePercent,
            RateType = e.RateType.DisplayName,
            TermYears = e.TermYears,
            ExpectedClosingOn = e.ExpectedClosingOn,
            BusinessDaysBeforeClosing = MortgageRequest.BusinessDaysBetween(e.ReceivedOn, e.ExpectedClosingOn),
            ClosingCosts = e.ClosingCosts?.Amount,
            Notes = e.Notes,
            RecordedAt = e.OccurredAt,
        };

    public static void Apply(MortgageRequestView view, MortgageRequestAppraisalRecorded e)
    {
        view.AppraisedValue = e.AppraisedValue.Amount;
        view.AppraisalDate = e.AppraisalDate;
        view.CurrentIndexValue = e.CurrentIndexValue;
        view.ClosingCosts = e.ClosingCosts?.Amount;
        view.PurchaseOptionPrice = e.PurchaseOptionPrice.Amount;
        view.MaxMortgageDebtPercent = e.MaxMortgageDebtPercent;
        view.DebtCapAmount = e.DebtCapAmount.Amount;
        view.DebtPercent = e.DebtPercent;
    }

    public static void Apply(MortgageRequestView view, MortgageRequestApproved e)
    {
        view.Status = MortgageRequestStatus.Approved.DisplayName;
        view.DecidedOn = e.ApprovedOn;
        view.DecisionReason = e.Reason;
        view.ApprovedOverCap = e.OverCap;
    }

    public static void Apply(MortgageRequestView view, MortgageRequestDenied e)
    {
        view.Status = MortgageRequestStatus.Denied.DisplayName;
        view.DecidedOn = e.DeniedOn;
        view.DecisionReason = e.Reason;
    }

    public static void Apply(MortgageRequestView view, MortgageRequestLoanClosed e)
    {
        view.Status = MortgageRequestStatus.Closed.DisplayName;
        view.ClosedOn = e.ClosedOn;
        view.PermittedMortgageId = e.PermittedMortgageId;
        view.PaidOffMortgageIds = [.. e.PaidOffMortgageIds];
    }

    public static void Apply(MortgageRequestView view, MortgageRequestWithdrawn e)
    {
        view.Status = MortgageRequestStatus.Withdrawn.DisplayName;
        view.WithdrawnOn = e.WithdrawnOn;
        view.WithdrawnReason = e.Reason;
    }
}
