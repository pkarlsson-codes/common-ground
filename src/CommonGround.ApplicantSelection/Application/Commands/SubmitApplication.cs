using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.ApplicantSelection.Application.Commands;

/// <summary>Submits a new homeowner application.</summary>
public sealed record SubmitApplication(
    Applicant Applicant,
    int HouseholdSize,
    decimal AnnualIncome,
    bool AccessibilityRequired,
    UserId SubmittedBy);

/// <summary>Handles <see cref="SubmitApplication"/>.</summary>
public static class SubmitApplicationHandler
{
    /// <summary>Creates a new homeowner application and returns its ID.</summary>
    public static HomeownerApplicationId Handle(
        SubmitApplication command,
        IDocumentSession session)
    {
        ArgumentNullException.ThrowIfNull(command);

        var application = HomeownerApplication.Submit(
            command.Applicant,
            new HouseholdInfo(command.HouseholdSize, command.AnnualIncome, command.AccessibilityRequired),
            command.SubmittedBy);

        session.CreateAggregate<HomeownerApplication, HomeownerApplicationId>(application);

        return application.Id;
    }
}