# Draw Selection Lottery

1. User opens an Open selection (see [View Home Selection](view-home-selection.md))
2. To break a tie, user ticks "Tied" on two or more candidates; to draw from everyone, user leaves them all unticked
3. User clicks "Draw Lottery From All" or "Draw Lottery Between N Tied"
4. "Draw Lottery" panel opens, saying whether it draws from all candidates or between the tied ones, and that candidates who can no longer be selected are left out and the seed is recorded so the draw can be checked
5. User searches for and selects an active Ground Lease Template, enters a Purchase Price (for a home in resale, prefilled with the Purchase Option Price and not higher), and optionally adjusts Selected Date (defaults to today)
6. User clicks "Draw Lottery" (disabled until a template, a price and a date are set)
7. Toast: "Lottery drawn"
8. Panel closes and the selection becomes Completed, showing the method, the seed, how many candidates were drawn from and how many were left out; the drawn household's row is marked "Selected"

The outcome for the drawn household, the home, the purchase workflow and the other candidates is the same as for [Select Household by Priority](select-household-by-priority.md): candidates who could be selected but weren't drawn are recorded as passed over, just after the draw completes.

If none of the candidates in the pool can be selected, the draw is rejected with "None of the candidates in the lottery can be selected right now."

Continues in: [View Purchase Workflow Detail](../purchase-workflows/view-purchase-workflow-detail.md)
