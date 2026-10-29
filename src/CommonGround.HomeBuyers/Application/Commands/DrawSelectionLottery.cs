using System.Security.Cryptography;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>
/// Chooses a candidate in a home selection by lottery, drawing from
/// <paramref name="TiedApplicationIds"/> when given, otherwise from every candidate.
/// </summary>
public sealed record DrawSelectionLottery(
    HomeSelectionId SelectionId,
    IReadOnlyList<HomeownerApplicationId>? TiedApplicationIds,
    UserId DrawnBy);

/// <summary>Handles <see cref="DrawSelectionLottery"/>.</summary>
public static class DrawSelectionLotteryHandler
{
    /// <summary>Loads the selection, draws with a fresh random seed, and persists it.</summary>
    public static async Task Handle(
        DrawSelectionLottery command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var selection = await session
            .LoadAggregateAsync<HomeSelection>(command.SelectionId, cancellationToken)
            .ConfigureAwait(false);

        var seed = RandomNumberGenerator.GetInt32(int.MaxValue);
        selection.DrawLottery(command.TiedApplicationIds, seed, command.DrawnBy);

        session.UpdateAggregate(selection);
    }
}
