# Deny Application

1. User navigates to Applications
2. User clicks a row for an application in the Submitted phase to open the detail panel
3. In Workflow Actions, user clicks "Deny Application" (red text, in the separate group below the other actions)
4. "Deny Application" panel opens with Denied Date defaulted to today
5. User enters a required Reason and optionally adjusts Denied Date
6. User clicks "Deny Application" (disabled until Reason and Denied Date are set)
7. A confirmation dialog asks "Deny this application? This action cannot be undone." User clicks "Deny Application" to confirm, or Cancel to return to the panel
8. Toast: "Application denied"
9. Panel closes, application moves to the Denied phase and no further workflow actions are available

Pressing Escape (or Close or Cancel) with typed input asks "Discard changes?" before closing. See [Close an overlay](../navigation/close-an-overlay.md).

An approved application can't be denied. To take an approved household off the list, see [Remove from Waiting List](remove-from-waiting-list.md).
