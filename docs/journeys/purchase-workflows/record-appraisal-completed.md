# Record Appraisal Completed

1. User navigates to Purchases
2. User clicks a row for a non-terminal workflow to open the detail panel
3. In Purchase Tracks, user expands the Appraisal track (only expandable while pending)
4. User enters Appraised Value ($, above zero), Appraisal Date and Appraiser Name (all required), and optional Notes. Appraised Value is the home's market value without the lease's restrictions, not the lender's value of the home with its leasehold. Appraised Value is prefilled from the resale's appraisal if the purchase is a resale buyer's, otherwise from the home's latest assessment, if it has one; user confirms or corrects it
5. User clicks "Record Appraisal"
6. Toast: "Appraisal recorded"
7. Track collapses and shows "{appraised value} · appraised {date} by {appraiser name}"; the Appraisal track is marked complete

When the purchase closes, the new ground lease takes this Appraised Value as its Original Appraised Value (see [Record Purchase Closed](record-purchase-closed.md)).
