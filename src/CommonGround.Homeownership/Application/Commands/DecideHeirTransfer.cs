using CommonGround.Homeownership.Application.Services;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.HeirTransfers;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records who is taking the home and how they qualify. Recording again replaces the heir.</summary>
/// <param name="HeirTransferId">The transfer.</param>
/// <param name="FullName">The heir's full name.</param>
/// <param name="Email">The heir's email address.</param>
/// <param name="Phone">The heir's phone number, if given.</param>
/// <param name="Relationship">How the heir qualifies.</param>
/// <param name="MovedInOn">The day a household member moved in; read for that relationship only.</param>
/// <param name="VerifiedAnnualIncome">The heir's verified annual income; read for any other heir only.</param>
/// <param name="VerifiedHouseholdSize">The heir's household size; read for any other heir only.</param>
/// <param name="IncomeDocumentIds">The income documents as Documents file IDs; read for any other heir only.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record RecordHeir(
    HeirTransferId HeirTransferId,
    string? FullName,
    string? Email,
    string? Phone,
    HeirRelationship Relationship,
    DateOnly? MovedInOn,
    Money? VerifiedAnnualIncome,
    int? VerifiedHouseholdSize,
    IReadOnlyList<string>? IncomeDocumentIds,
    UserId RecordedBy);

/// <summary>Records the trust's consent once the heir has signed the letters; the lease then names the heir as homeowner.</summary>
/// <param name="HeirTransferId">The transfer.</param>
/// <param name="TransferredOn">The day the home passed to the heir.</param>
/// <param name="LetterOfAgreementDocumentId">The signed Letter of Agreement, as a Documents file ID.</param>
/// <param name="AttorneyAcknowledgementDocumentId">The signed Attorney's Acknowledgment, as a Documents file ID.</param>
/// <param name="Notes">Anything staff want to note.</param>
/// <param name="CompletedBy">The staff user recording it.</param>
public sealed record CompleteHeirTransfer(
    HeirTransferId HeirTransferId,
    DateOnly TransferredOn,
    string? LetterOfAgreementDocumentId,
    string? AttorneyAcknowledgementDocumentId,
    string? Notes,
    UserId CompletedBy);

/// <summary>Closes the transfer without the home passing to the heir. The lease is unchanged.</summary>
/// <param name="HeirTransferId">The transfer.</param>
/// <param name="Reason">Why it closed.</param>
/// <param name="Notes">Anything staff want to note.</param>
/// <param name="ClosedBy">The staff user closing it.</param>
public sealed record CloseHeirTransfer(HeirTransferId HeirTransferId, HeirTransferCloseReason Reason, string? Notes, UserId ClosedBy);

/// <summary>What recording an heir decides from.</summary>
/// <param name="Transfer">The transfer stream, fetched for writing.</param>
/// <param name="IncomeLimit">The home's income limit on the notice date; null when the home has none or the heir isn't checked.</param>
public sealed record HeirRecording(IEventStream<HeirTransfer> Transfer, HeirIncomeLimit? IncomeLimit);

/// <summary>What completing a transfer decides from.</summary>
/// <param name="Transfer">The transfer stream, fetched for writing.</param>
/// <param name="Lease">The lease stream, fetched for writing.</param>
/// <param name="Today">The trust's today.</param>
public sealed record HeirTransferCompletion(IEventStream<HeirTransfer> Transfer, IEventStream<GroundLease> Lease, DateOnly Today);

/// <summary>Handles <see cref="RecordHeir"/>.</summary>
public static class RecordHeirHandler
{
    /// <summary>Fetches the transfer for writing and, for any other heir, the home's income limit on the notice date.</summary>
    public static async Task<HeirRecording> LoadAsync(
        RecordHeir command,
        IDocumentSession session,
        IHomeIncomeLimits incomeLimits,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(incomeLimits);

        var transfer = await session.Events
            .FetchForWriting<HeirTransfer>(command.HeirTransferId.Value, cancellationToken)
            .ConfigureAwait(false);
        var aggregate = transfer.Aggregate
            ?? throw new AggregateNotFoundException(typeof(HeirTransfer), command.HeirTransferId.Value);

        if (command.Relationship != HeirRelationship.Other)
            return new HeirRecording(transfer, null);

        var lease = await session.LoadAggregateAsync<GroundLease>(aggregate.GroundLeaseId.Value, cancellationToken).ConfigureAwait(false);
        var limit = await incomeLimits.FindAsync(session, lease.UnitId, aggregate.NoticeReceivedOn, cancellationToken).ConfigureAwait(false);

        return new HeirRecording(transfer, limit);
    }

    /// <summary>Records the heir.</summary>
    public static void Handle(RecordHeir command, HeirRecording recording)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(recording);

        recording.Transfer.Decide(t => t.RecordHeir(
            command.FullName, command.Email, command.Phone, command.Relationship, command.MovedInOn,
            command.VerifiedAnnualIncome, command.VerifiedHouseholdSize, command.IncomeDocumentIds,
            recording.IncomeLimit, command.RecordedBy));
    }
}

/// <summary>Handles <see cref="CompleteHeirTransfer"/>.</summary>
public static class CompleteHeirTransferHandler
{
    /// <summary>Fetches the transfer and its lease for writing, and the trust's today.</summary>
    public static async Task<HeirTransferCompletion> LoadAsync(
        CompleteHeirTransfer command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var transfer = await session.Events
            .FetchForWriting<HeirTransfer>(command.HeirTransferId.Value, cancellationToken)
            .ConfigureAwait(false);
        var leaseId = transfer.Aggregate?.GroundLeaseId
            ?? throw new AggregateNotFoundException(typeof(HeirTransfer), command.HeirTransferId.Value);
        var lease = await session.Events
            .FetchForWriting<GroundLease>(leaseId.Value, cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new HeirTransferCompletion(transfer, lease, today);
    }

    /// <summary>Completes the transfer, then passes the home to the heir on the lease, in the same session. The lease reads the heir the
    /// transfer already had; completing doesn't change it.</summary>
    public static void Handle(CompleteHeirTransfer command, HeirTransferCompletion completion)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(completion);

        completion.Transfer.Decide(t => t.Complete(
            command.TransferredOn, command.LetterOfAgreementDocumentId, command.AttorneyAcknowledgementDocumentId,
            command.Notes, completion.Today, command.CompletedBy));
        completion.Lease.Decide(l => l.PassToHeir(completion.Transfer.Aggregate!, command.TransferredOn, command.CompletedBy));
    }
}

/// <summary>Handles <see cref="CloseHeirTransfer"/>.</summary>
public static class CloseHeirTransferHandler
{
    /// <summary>Fetches the transfer for writing.</summary>
    public static Task<IEventStream<HeirTransfer>> LoadAsync(CloseHeirTransfer command, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);

        return session.Events.FetchForWriting<HeirTransfer>(command.HeirTransferId.Value, cancellationToken);
    }

    /// <summary>Closes the transfer.</summary>
    public static void Handle(CloseHeirTransfer command, IEventStream<HeirTransfer> transfer)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(transfer);

        transfer.Decide(t => t.Close(command.Reason, command.Notes, command.ClosedBy));
    }
}
