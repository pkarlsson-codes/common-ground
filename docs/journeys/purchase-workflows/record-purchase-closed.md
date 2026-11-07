# Record Purchase Closed

1. User navigates to Purchases
2. User clicks a row for a workflow in the Closing Scheduled phase to open the detail panel
3. In the Closing track, user enters Actual Closing Date (defaulted to today, required) and optional Closing Notes
4. User clicks "Record Purchase Closed"
5. Toast: "Purchase closed"
6. The workflow moves to the Closed phase; Closing Notes (if entered) are shown, and Purchase Tracks and Exception Actions are no longer shown
7. In the background, a new ground lease is created from the workflow's lease template, with the buyer as homeowner, the lease start date equal to the actual closing date, and the purchase's Purchase Price and the Appraised Value from its Appraisal track as the lease's Base Price and Original Appraised Value (the resale formula starts from them); it appears in the Ground Leases table (lease creation is rejected if the template has been archived in the meantime)
8. In the background, the home moves to the Leased stage, and if the workflow was initiated by selecting an applicant for a home, the household moves to the Homeowner stage
9. If the home was in resale, the seller's ground lease ends and the resale closes as Sold
10. If the workflow was initiated by selecting an applicant for a home, its application and household remain in Connected Records after closing

Continues in: [View Ground Lease Detail](../ground-leases/view-ground-lease-detail.md)
