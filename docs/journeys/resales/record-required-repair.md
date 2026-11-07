# Record Required Repair

Not part of the first resale slice. Nothing links here yet; the resale journeys add a Repairs tracker step and a Repairs section when this is built, and "Record Closing" then waits for outstanding repairs.

For repairs the homeowner must make before transfer (inspection findings under the lease's transfer article). The trust cannot close the buyback while any repair is outstanding.

1. User opens a resale whose option is exercised (see [View Resale Detail](view-resale-detail.md)) and clicks "Record Repair" on the Next Step card, or "Add Repair" in the Repairs section
2. "Record Repair" panel opens
3. User enters a Description (required) and an Estimated Cost (required, zero or more)
4. User clicks "Record Repair", or "Cancel"
5. Toast: "Repair recorded"
6. The repair appears in the Repairs section as Outstanding, and the Next Step card counts it
7. To mark a repair done, user clicks "Mark Done" on its row, enters Completed Date (default today, not in the future) and optional Actual Cost, then clicks "Mark Done". Toast: "Repair completed"
8. When every repair is done, or if none were recorded, the Next Step card offers "Record Closing"

Continues in: [Record Buyback Closed](record-buyback-closed.md)
