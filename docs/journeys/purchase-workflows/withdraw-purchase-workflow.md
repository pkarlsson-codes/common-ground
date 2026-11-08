# Withdraw Purchase Workflow

1. User navigates to Purchases
2. User clicks a row for a non-terminal workflow to open the detail panel
3. In Exception Actions, user enters a required Withdrawal Reason
4. User clicks "Withdraw Purchase"
5. Confirmation dialog: "Withdraw Purchase" — "Withdraw this purchase workflow? This action cannot be undone."
6. User clicks "Withdraw Purchase" to confirm (or "Cancel" to back out)
7. Toast: "Workflow withdrawn"
8. Panel shows Withdrawal Reason; the workflow moves to the Withdrawn phase and Purchase Tracks and Exception Actions are no longer shown
9. If the workflow was initiated by selecting an applicant for a home, in the background the home goes back to the Selection stage with a new home selection started for it, the completed selection is marked Fell Through, and the household's application returns to the waiting list with its original approved date

Closing the purchase panel with a reason typed asks "Discard changes?" first. See [Close an overlay](../navigation/close-an-overlay.md).

Continues in: [View Home Selection](../home-selections/view-home-selection.md)
