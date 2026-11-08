# Fail Purchase Workflow

1. User navigates to Purchases
2. User clicks a row for a non-terminal workflow to open the detail panel
3. In Exception Actions, user enters a required Fail Reason
4. User clicks "Mark as Failed"
5. Confirmation dialog: "Mark as Failed" — "Mark this workflow as failed? This action cannot be undone."
6. User clicks "Mark as Failed" to confirm (or "Cancel" to back out)
7. Toast: "Workflow marked failed"
8. Panel shows Failure Reason; the workflow moves to the Failed phase and Purchase Tracks and Exception Actions are no longer shown
9. If the workflow was initiated by selecting an applicant for a home, in the background the home goes back to the Selection stage with a new home selection started for it, the completed selection is marked Fell Through, and the household's application returns to the waiting list with its original approved date

Closing the purchase panel with a reason typed asks "Discard changes?" first. See [Close an overlay](../navigation/close-an-overlay.md).

Continues in: [View Home Selection](../home-selections/view-home-selection.md)
