using CommonGround.PropertyRegistry.Domain.DevelopmentPlans;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Initiates a new development plan for a land parcel.</summary>
/// <param name="LandParcelId">The parcel to associate the plan with.</param>
/// <param name="Description">A description of the development plan.</param>
/// <param name="InitiatedBy">The staff user initiating the plan.</param>
public sealed record InitiateDevelopmentPlan(
    LandParcelId LandParcelId,
    string Description,
    UserId InitiatedBy);

/// <summary>Handles <see cref="InitiateDevelopmentPlan"/> by creating and persisting a new DevelopmentPlan.</summary>
public static class InitiateDevelopmentPlanHandler
{
    /// <summary>Creates a new DevelopmentPlan, persists it, and returns its ID.</summary>
    public static async Task<DevelopmentPlanId> Handle(
        InitiateDevelopmentPlan command,
        IDevelopmentPlanRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var plan = DevelopmentPlan.Initiate(
            command.LandParcelId,
            command.Description,
            command.InitiatedBy);

        repository.Stage(plan);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return plan.Id;
    }
}
