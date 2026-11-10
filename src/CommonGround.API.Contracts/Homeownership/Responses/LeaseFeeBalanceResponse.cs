namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>A lease and what it owes in lease fees as of the trust's today.</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="HomeId">The leased home.</param>
/// <param name="HomeLabel">The home's label, if known.</param>
/// <param name="HomeownerFullName">The homeowner's name.</param>
/// <param name="LeaseStatus">Active or Inactive.</param>
/// <param name="MonthlyFee">The fee charged on the 1st of each month.</param>
/// <param name="Balance">Above zero a balance due, below zero a credit.</param>
/// <param name="OldestUnpaidDueDate">Due date of the oldest charge not fully paid, if any.</param>
/// <param name="DaysOverdue">Days since that charge fell due; zero when nothing is owed.</param>
public sealed record LeaseFeeBalanceResponse(
    string GroundLeaseId,
    string HomeId,
    string? HomeLabel,
    string? HomeownerFullName,
    string LeaseStatus,
    decimal MonthlyFee,
    decimal Balance,
    DateOnly? OldestUnpaidDueDate,
    int DaysOverdue);
