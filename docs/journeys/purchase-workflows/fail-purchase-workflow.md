# Fail Purchase Workflow

1. User navigates to Purchases
2. User clicks a row for a non-terminal workflow to open the detail panel
3. In Exception Actions, user enters a required Fail Reason
4. User clicks "Mark as Failed"
5. Confirmation dialog: "Mark as Failed" — "Mark this workflow as failed? This action cannot be undone."
6. User clicks "Mark as Failed" to confirm (or "Cancel" to back out)
7. Toast: "Workflow marked failed"
8. Panel shows Failure Reason; the workflow moves to the Failed phase and Purchase Tracks and Exception Actions are no longer shown
9. If the workflow was initiated by selecting an applicant for a home, in the background the household's application returns to the waiting list with its original approved date, and the home becomes available again and appears on Today as a home available again after a purchase fell through. No new selection is started: staff choose the next household from the home's selection page, and the household whose purchase fell through is on the waiting list for it too. The record of the earlier choice does not change

Closing the purchase panel with a reason typed asks "Discard changes?" first. See [Close an overlay](../navigation/close-an-overlay.md).

Continues in: [View Home Selection](../home-selections/view-home-selection.md)
