# Draw Selection Lottery

1. User opens the selection page of a home that is offered to the waiting list (see [View Home Selection](view-home-selection.md))
2. To break a tie, user ticks "Tied" on two or more households; to draw from the whole waiting list, user leaves them all unticked
3. User clicks "Draw Lottery From All" or "Draw Lottery Between N Tied"
4. "Draw Lottery" panel opens, saying whether it draws from the whole waiting list or between the tied households, and that households that can't be chosen on the date of the draw are left out of it and the whole waiting list and the seed are recorded so the draw can be checked
5. User searches for and selects an active Ground Lease Template, enters a Purchase Price, and optionally changes the Date of the Choice (defaults to today; can't be after today)
6. User clicks "Draw Lottery" (disabled until a template, a price and a date are set)
7. Toast: "Lottery drawn"
8. Panel closes. The page shows the draw, with the method, how many households it was drawn from, how many tied households were left out, the seed, and the whole waiting list with each household marked Chosen, Passed Over, or Couldn't Be Chosen

The draw is one step, like [Select Household by Priority](select-household-by-priority.md), and has the same outcome for the drawn household, the home, the purchase workflow and the households passed over. Households outside the tie that could have been chosen are passed over too. The same refusals apply. A draw between tied households also needs at least two of them, and is refused when none of the households in it can be chosen: "None of the households in the lottery can be chosen on that date."

Continues in: [View Purchase Workflow Detail](../purchase-workflows/view-purchase-workflow-detail.md)
