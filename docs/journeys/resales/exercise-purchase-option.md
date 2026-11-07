# Exercise Purchase Option

For when the trust decides to buy the home itself at the Purchase Option Price. To let another buyer have the home instead, [decline the option](decline-purchase-option.md).

1. User opens a resale that has an appraisal and no decision (see [View Resale Detail](view-resale-detail.md)) and clicks "Buy the Home"
2. "Buy the Home" panel opens, showing the Purchase Option Price, the Option Window End and the days left
3. User enters Exercised Date (default today, not in the future, not before the appraisal date) and optional Notes
4. User clicks "Exercise Option" (disabled until the date is valid), or "Cancel"
5. Toast: "Purchase option exercised"
6. The tracker moves to Closing, and the Next Step card offers "Record Closing"
7. The resale can no longer be withdrawn from the Management app

If the Exercised Date is after the Option Window End, the panel is rejected with "The option window ended on {date}."

Continues in: [Record Buyback Closed](record-buyback-closed.md)
