using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>
/// Applies purchase-closing outcomes to the linked ground lease.
/// On close, the workflow buyer becomes the lease homeowner.
/// </summary>
public sealed class PurchaseClosedHandler(
    IPurchaseWorkflowRepository workflowRepository,
    IGroundLeaseRepository leaseRepository,
    IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Handles purchase closure by onboarding or changing the ground lease homeowner.
    /// </summary>
    /// <param name="event">The purchase closed domain event.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(PurchaseClosed @event, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var workflow = await workflowRepository
            .GetByIdAsync(PurchaseWorkflowId.Create(@event.AggregateId), ct)
            .ConfigureAwait(false)
            ?? throw new SharedKernel.Domain.Exceptions.AggregateNotFoundException(@event.AggregateId);

        var lease = await leaseRepository
            .GetByIdAsync(workflow.GroundLeaseId, ct)
            .ConfigureAwait(false)
            ?? throw new SharedKernel.Domain.Exceptions.AggregateNotFoundException(workflow.GroundLeaseId.Value);

        var recordedBy = UserId.Create(@event.RecordedBy);
        var buyer = workflow.Buyer;

        if (lease.CurrentHomeowner is null)
        {
            lease.OnboardHomeowner(buyer, recordedBy);
        }
        else if (!IsSameHomeowner(lease.CurrentHomeowner, buyer))
        {
            lease.ChangeHomeowner(buyer, recordedBy);
        }
        else
        {
            return;
        }

        leaseRepository.Stage(lease);
        await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
    }

    private static bool IsSameHomeowner(Homeowner current, Homeowner buyer)
    {
        return string.Equals(current.FullName, buyer.FullName, StringComparison.Ordinal)
            && string.Equals(current.Email, buyer.Email, StringComparison.OrdinalIgnoreCase)
            && string.Equals(current.Phone, buyer.Phone, StringComparison.Ordinal);
    }
}