# Record Resale Appraisal

For when the trust has the appraisal the lease requires after a notice of intent to sell. It sets the Purchase Option Price the trust pays.

1. User opens a resale that has no appraisal (see [View Resale Detail](view-resale-detail.md)) and clicks "Record Appraisal" on the Next Step card
2. "Record Appraisal" panel opens with the home's label as subtitle
3. User enters Appraised Value (required, above zero), Appraisal Date (default today, not in the future, not before the notice date) and Appraiser (required, free text)
4. If the lease's formula credits improvements, user enters Approved Improvements (default 0). If the formula is Index Based, user enters Current Index Value (required, above zero)
5. The panel previews the result as the user types: Formula Price, Appraised Value, and the Purchase Option Price (the lesser of the two)
6. User clicks "Record Appraisal" (disabled until the required fields are valid), or "Cancel"
7. Toast: "Appraisal recorded"
8. The resale's Appraisal and Price sections appear, and the Next Step card asks for the decision

An appraisal can be corrected until the option is exercised or declined: "Correct Appraisal" in the resale's Appraisal section opens this panel again. Recording a new one replaces the earlier one in the Price section and keeps it in the resale's history.

The Formula Price needs the lease's Base Price and Original Appraised Value, which every lease has from [Create Ground Lease](../ground-leases/create-ground-lease.md). If a lease somehow lacks them, the panel says so and "Record Appraisal" stays disabled.

Continues in: [Exercise Purchase Option](exercise-purchase-option.md)
Continues in: [Decline Purchase Option](decline-purchase-option.md)
