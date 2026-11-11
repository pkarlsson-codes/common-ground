# Correct Lease Fee Tracking

For a lease whose fees-tracked-from date or opening balance was entered wrong, at creation or by a portfolio import. A lease tracked from its start when it should be tracked from this month shows years of fees as unpaid.

1. User opens a ground lease (see [View Ground Lease Detail](view-ground-lease-detail.md)). The Lease Fees section has a "Correct Fee Tracking" button
2. User clicks "Correct Fee Tracking". The "Correct Fee Tracking" panel opens with the homeowner as subtitle, and says months before the day aren't charged, the amount owed is due on that day, and a payment received before the day must be voided first
3. Fees Tracked From starts at the lease's current tracked-from date (the lease start for a lease tracked from its start). Before the lease start, the field says "Can't be before the lease start, {date}." and Save is disabled
4. Owed on That Day ($) starts at the current opening balance
5. User clicks "Save" (disabled until a value changes), or "Cancel"
6. A payment that still counts and was received before the new day refuses the save: "A payment was received before that day. Void it first, or choose a day on or before it."
7. Toast: "Fee tracking corrected"
8. The balance, oldest unpaid month and Fees Tracked From show the new tracking. The Lease Fees tab and Today use it too
