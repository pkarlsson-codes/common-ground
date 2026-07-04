# Record Purchase Closed

1. User navigates to Purchase Workflows
2. User clicks a row for a workflow in the Closing Scheduled phase to open the detail panel
3. In the Closing track, user enters Actual Closing Date (defaulted to today, required) and optional Closing Notes
4. User clicks "Record Purchase Closed"
5. Toast: "Purchase closed"
6. Closing track shows "Closed {date}"; the workflow moves to the Closed phase and Purchase Tracks and Exception Actions are no longer shown
7. In the background, a new ground lease is created from the workflow's lease template, with the buyer as homeowner and the lease start date equal to the actual closing date; it appears in the Ground Leases table (lease creation is rejected if the template has been archived in the meantime)
8. If the workflow was initiated by selecting an applicant for a home, the "Open Application" link remains available in the detail panel after closing

Continues in: [View Ground Lease Detail](../ground-leases/view-ground-lease-detail.md)
