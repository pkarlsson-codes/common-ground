# Send Resale Price Statements

For the trust's yearly round: print a resale price statement for every lease that is due, post them, and record them sent. For one lease, see [Prepare Resale Price Statement](../ground-leases/prepare-resale-price-statement.md).

1. User navigates to Stewardship and clicks the Resale Prices tab (after Lease Fees)
2. The table lists active leases with no resale in progress: Homeowner, Home, Formula, Last Statement (date, or "None") and Next Due (a year after the last statement, or after the lease start when none was sent; red when passed). The filter is "Due" (default: Next Due is today or earlier) or "All leases". The search box filters by homeowner name or home label. With Due and none due: "No statements are due."
3. Clicking a row opens the ground lease in its detail panel
4. With Due selected and at least one due, user clicks "Prepare Statements"
5. "Prepare Statements" panel opens with Statement Date (default today) and the figures the round needs:
   - One value per index type among the index-based leases due ("CPI-U value on the statement date"), with each baseline shown
   - One row per appraisal-based lease due: homeowner, home, Market Value and Value Source; Value Source fills down from the row above when left blank
6. Below, every lease due with its Maximum Resale Price once its figures are in. A lease that can't be priced (an appraisal-based lease with no original appraised value) shows why, is unticked, and can't be ticked. Each other lease has a tick, checked. Improvements credits aren't asked; a lease that needs one gets its statement from the lease
7. User clicks "Print Statements". A print page opens in a new tab with one letter per ticked lease, a page each, as in [Prepare Resale Price Statement](../ground-leases/prepare-resale-price-statement.md), and the browser's print dialog. The button is disabled until every ticked lease has its figures
8. User posts or hands over the letters, unticks any not sent, sets Delivery (default First-Class Mail) and clicks "Record N Statements Sent", or "Cancel"
9. Toast: "N statements recorded". The leases leave the Due list. If a statement can't be recorded (for example a resale started in the meantime), the panel stays open and lists which leases failed and why; the rest are recorded

Continues in: [View Ground Lease Detail](../ground-leases/view-ground-lease-detail.md)
