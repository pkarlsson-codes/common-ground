# View Resale Detail

1. User navigates to Resales and clicks a row, or clicks "Open Resale" in the Resale section of a ground lease or on the home's Next Step card
2. Detail panel opens titled with the home's label, with the homeowner's name as subtitle
3. A step tracker shows Notice → Appraisal → Decision → Closing when the trust buys the home, or Notice → Appraisal → Decision → Buyer → Closing when it declines: steps already done are marked with their date, the current step is highlighted, later steps are dimmed. A withdrawn resale ends the tracker at that step
4. A Next Step card under the tracker says in one sentence what the resale is waiting on, with one primary button:
   - Notice given, no appraisal: "Record the appraisal.", with "Record Appraisal"
   - Appraised, no decision: "Decide whether the trust buys this home by {Option Window End}." (or "The option window has passed" once it has), with "Buy the Home" and "Decline to Buy"
   - Option exercised: "Record the closing when the trust has paid the Purchase Option Price.", with "Record Closing"
   - Declined, no selection: "The trust declined to buy this home on {date}. Choose a buyer from the waiting list.", with "Start Selection"
   - Declined, selection open: "Choosing a buyer.", with "Open Selection"
   - Declined, buyer's purchase underway: the purchase's current step (e.g. "Waiting on the inspection"), with "Open Purchase"
   - Closed: "The trust bought this home on {date}." or "Sold to {buyer} on {date}."
5. Panel shows the notice (Notice Date, how it came in, notes), the Option Window End and the days left, or "Window passed"
6. Once an appraisal is recorded, panel shows an Appraisal section (Appraised Value, Appraisal Date, Appraiser, Approved Improvements and Current Index Value when the lease's formula uses them) and a Price section: Formula Price, Appraised Value and the Purchase Option Price, which is the lesser of the two
7. Once the option is exercised, panel shows the Exercised Date
8. A resale the trust bought shows the Closing Date, Purchase Price Paid and Closing Notes. A resale sold to a buyer shows the Closing Date and the sale price
9. A Connected Records section links to the ground lease, the home, the home selection and the buyer's purchase (once they exist) and, after closing, the ended lease and the buyer's new lease
10. "Withdraw" is available in the panel's actions until the trust exercises the option or a household is selected as the buyer
11. User closes the panel

Not built yet: "Decline to Buy" and the Buyer step. Until [Decline Purchase Option](decline-purchase-option.md) is built, the Decision step offers only "Buy the Home", and the tracker ends at Closing for the trust's purchase.

Continues in: [Record Resale Appraisal](record-resale-appraisal.md)
Continues in: [Exercise Purchase Option](exercise-purchase-option.md)
Continues in: [Decline Purchase Option](decline-purchase-option.md)
Continues in: [Record Buyback Closed](record-buyback-closed.md)
Continues in: [Start Home Selection](../home-selections/start-home-selection.md)
Continues in: [View Purchase Workflow Detail](../purchase-workflows/view-purchase-workflow-detail.md)
Continues in: [Withdraw Intent to Sell](../ground-leases/withdraw-intent-to-sell.md)
Continues in: [View Ground Lease Detail](../ground-leases/view-ground-lease-detail.md)
Continues in: [View Home Detail](../homes/view-home-detail.md)
