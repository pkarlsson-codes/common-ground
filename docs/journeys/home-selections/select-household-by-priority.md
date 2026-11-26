# Select Household by Priority

1. User opens the selection page of a home that is offered to the waiting list (see [View Home Selection](view-home-selection.md)) and compares the households
2. User clicks "Choose" on a household that can be chosen
3. "Choose by Priority" panel opens with the household's name as subtitle
4. User ticks one or more Priorities That Justify This Choice: Lower income, Household size fit, Accessibility need, Waiting-list tenure
5. User enters a Reason explaining why this household over the others on the waiting list
6. User searches for and selects an active Ground Lease Template, enters a Purchase Price, and optionally changes the Date of the Choice (defaults to today; can't be after today)
7. User clicks "Choose Household" (disabled until at least one priority, a reason, a template, a price and a date are set)
8. Toast: "Household chosen"
9. Panel closes. The page shows the choice, newest first, with the whole waiting list as it was compared, and the Waiting List Today section says a household has been chosen

Choosing is one step. It compares the waiting list as it is against the home on the Date of the Choice, judging income limits and prequalifications as of that date, and records every application on the list with whether it was chosen, passed over, or couldn't be chosen and why. An application approved after that date wasn't waiting yet, so it is left out.

When the household is chosen:
- the chosen application moves to Selected and leaves the waiting list, and the home moves to Under Contract
- a purchase workflow is created for the household, the home and the terms entered (see [Initiate Purchase Workflow](../purchase-workflows/initiate-purchase-workflow.md)). The home can't then be leased by hand
- every application the choice marked passed over is recorded as passed over and stays on the waiting list. Households that couldn't be chosen are not passed over. Pass-overs are recorded just after the choice, so a household's Passed Over count can take a moment to catch up

The choice is refused when:
- the home isn't offered: "A household can only be chosen for a home that is offered to the waiting list." A second choice for the same home is refused this way, including one made at the same moment as the first
- the household can't be chosen on the date, for example its prequalification expired before it: "This household can't be chosen on that date: …"
- the date is after today, or before the home became available again
- the home has an income limit and no income limits in effect on the date have its level: "No income limits for 80% of AMI are in effect on Nov 12, 2026. Enter them under Income Limits."
- nobody is on the waiting list

Continues in: [View Purchase Workflow Detail](../purchase-workflows/view-purchase-workflow-detail.md)
