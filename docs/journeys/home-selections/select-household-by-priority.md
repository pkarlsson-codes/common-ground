# Select Household by Priority

1. User opens an Open selection (see [View Home Selection](view-home-selection.md)) and compares the candidates' facts
2. User clicks "Select" on a candidate who can be selected
3. "Select by Priority" panel opens with the candidate's name as subtitle
4. User ticks one or more Priorities That Justify This Choice: Lower income, Household size fit, Accessibility need, Waiting-list tenure
5. User enters a Reason explaining why this household over the other candidates
6. User searches for and selects an active Ground Lease Template, enters a Purchase Price (for a home in resale, prefilled with the Purchase Option Price and not higher), and optionally adjusts Selected Date (defaults to today)
7. User clicks "Select Household" (disabled until at least one priority, a reason, a template, a price and a date are set)
8. Toast: "Household selected"
9. Panel closes and the selection becomes Completed, showing the outcome with the cited priorities and reason

When the selection completes:
- the chosen application moves to Selected and leaves the waiting list, and the home moves to UnderContract
- a purchase workflow is created for the household, the home and the terms entered (see [Initiate Purchase Workflow](../purchase-workflows/initiate-purchase-workflow.md))
- every other candidate who could still be selected is recorded as passed over and stays on the waiting list. Candidates who couldn't be selected at that moment (not ready, withdrawn, denied) are not passed over. Pass-overs are recorded just after the selection completes, so a candidate's Passed Over count can take a moment to catch up

The selection is rejected if the household can no longer be selected when the user submits, for example because its readiness is incomplete or its prequalification expires before the Selected Date.

Continues in: [View Purchase Workflow Detail](../purchase-workflows/view-purchase-workflow-detail.md)
