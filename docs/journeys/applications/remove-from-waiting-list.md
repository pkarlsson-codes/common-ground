# Remove from Waiting List

1. User navigates to Applications
2. User clicks a row for an application in the Approved phase to open the detail panel
3. In Workflow Actions, user clicks "Remove from Waiting List" (red text, in the separate group below the other actions)
4. "Remove from Waiting List" panel opens with Removed Date defaulted to today
5. User picks a required Reason: No longer eligible (income, household size or other eligibility changed), No response (the trust couldn't reach the household), False information (what the household declared turned out to be false), or Other
6. User enters a Note, required when Reason is Other and optional otherwise, and optionally adjusts Removed Date. The date can't be in the future or before the application's approved date
7. User clicks "Remove" (disabled until Reason and Removed Date are set, and a Note when Reason is Other)
8. A confirmation dialog asks "Remove this household from the waiting list? This action cannot be undone." User clicks "Remove" to confirm, or Cancel to return to the panel
9. Toast: "Removed from waiting list"
10. Panel closes, application moves to the Removed phase and no further workflow actions are available. The household is off the waiting list and is not a candidate in the next home selection

A removed household can apply again: staff [submit a new application](submit-application.md) for the existing household. Its Passed Over count carries over.

Pressing Escape (or Close or Cancel) with typed input asks "Discard changes?" before closing. See [Close an overlay](../navigation/close-an-overlay.md).
