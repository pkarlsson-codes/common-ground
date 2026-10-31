# Fail Purchase Workflow

1. User navigates to Purchases
2. User clicks a row for a non-terminal workflow to open the detail panel
3. In Exception Actions, user enters a required Fail Reason
4. User clicks "Mark as Failed"
5. Confirmation dialog: "Mark as Failed" — "Mark this workflow as failed? This action cannot be undone."
6. User clicks "Mark as Failed" to confirm (or "Cancel" to back out)
7. Toast: "Workflow marked failed"
8. Panel shows Failure Reason; the workflow moves to the Failed phase and Purchase Tracks and Exception Actions are no longer shown
