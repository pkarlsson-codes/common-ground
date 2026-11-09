# Withdraw Application

1. User navigates to Applications
2. User clicks a row for an application in the Submitted or Approved phase to open the detail panel
3. In Workflow Actions, user clicks "Withdraw Application" (red text, in the separate group below the other actions)
4. "Withdraw Application" panel opens with Withdrawn Date defaulted to today
5. User optionally enters a Reason and adjusts Withdrawn Date
6. User clicks "Withdraw" (disabled until Withdrawn Date is set)
7. A confirmation dialog asks "Withdraw this application? This action cannot be undone." User clicks "Withdraw" to confirm, or Cancel to return to the panel
8. Toast: "Application withdrawn"
9. Panel closes, application moves to the Withdrawn phase and no further workflow actions are available

Pressing Escape (or Close or Cancel) with typed input asks "Discard changes?" before closing. See [Close an overlay](../navigation/close-an-overlay.md).
