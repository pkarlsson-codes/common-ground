using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.ImprovementRequests;
using CommonGround.Homeownership.Domain.ImprovementRequests.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Homeownership.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="ImprovementRequestView"/> from ImprovementRequest events.</summary>
internal sealed class ImprovementRequestViewProjection : SingleStreamProjection<ImprovementRequestView, string>
{
    public static ImprovementRequestView Create(ImprovementRequestReceived e) =>
        new()
        {
            Id = e.AggregateId,
            GroundLeaseId = e.GroundLeaseId,
            HomeId = e.HomeId,
            HomeownerFullName = e.HomeownerFullName,
            Status = ImprovementRequestStatus.Waiting.DisplayName,
            ReceivedOn = e.ReceivedOn,
            ReplyDueOn = e.ReceivedOn.AddDays(ImprovementRequest.ReplyDays),
            Description = e.Description,
            EstimatedCost = e.EstimatedCost.Amount,
            RecordedAt = e.OccurredAt,
        };

    public static void Apply(ImprovementRequestView view, ImprovementRequestApproved e)
    {
        view.Status = ImprovementRequestStatus.Approved.DisplayName;
        view.ApprovedOn = e.ApprovedOn;
        view.PreApprovedCredit = e.PreApprovedCredit.Amount;
        view.LeaseImprovementsCap = e.LeaseImprovementsCap?.Amount;
        view.CreditOnLeaseBefore = e.CreditOnLeaseBefore.Amount;
        view.ApprovalNotes = e.Notes;
    }

    public static void Apply(ImprovementRequestView view, ImprovementRequestDenied e)
    {
        view.Status = ImprovementRequestStatus.Denied.DisplayName;
        view.DeniedOn = e.DeniedOn;
        view.DenialReason = e.Reason;
    }

    public static void Apply(ImprovementRequestView view, ImprovementWorkCompleted e)
    {
        view.Status = ImprovementRequestStatus.Completed.DisplayName;
        view.CompletedOn = e.CompletedOn;
        view.ActualCost = e.ActualCost.Amount;
        view.ReceiptDocumentIds = [.. e.ReceiptDocumentIds];
        view.ConfirmedCredit = e.ConfirmedCredit.Amount;
        view.CompletionNotes = e.Notes;
    }

    public static void Apply(ImprovementRequestView view, ImprovementRequestWithdrawn e)
    {
        view.Status = ImprovementRequestStatus.Withdrawn.DisplayName;
        view.WithdrawnOn = e.WithdrawnOn;
        view.WithdrawnReason = e.Reason;
    }
}
