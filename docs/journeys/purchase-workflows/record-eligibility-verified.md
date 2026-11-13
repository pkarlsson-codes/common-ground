# Record Eligibility Verified

Before closing, staff check the buyer's documents (pay stubs, tax returns) against the income limit the home's selection judged by. A purchase started from a selection carries that limit: the home's percent of AMI, the schedule's effective date and the schedule's amounts for that percent. Correcting the schedule or the home's limit later does not change it. The selection also keeps every level of that schedule, so the app can record which income band the buyer falls in for the [funder report](../reports/view-funder-report.md).

1. User navigates to Purchases
2. User clicks a row for a non-terminal workflow to open the detail panel
3. In Purchase Tracks, user expands the Eligibility track (only expandable while pending)
4. The track shows the income limit the selection judged by ("80% of AMI, limits effective Apr 1, 2026"), or "No income limit" when the home had none or the purchase didn't start from a selection
5. User enters Verified Date (defaulted to today), Verified Annual Income and Verified Household Size (both defaulted to what the household declared when selected; both required) and optional Notes
6. When the purchase has a limit, the track shows the limit for the entered household size ("Limit for 3 people: $81,600"), with "Over income limit" when the entered income is higher. Above 8 people it shows "Limit for 9 people: worked out when recorded"
   - When the purchase carries a schedule, the track also shows the band for the entered income and size: "Up to 50% of AMI", "Over 50% to 80% of AMI", or "Over 120% of AMI" when the income is above the highest level
7. User clicks "Record Eligibility"
8. Toast: "Eligibility recorded"
9. Track collapses and shows "Verified {date}: $72,000 income, 3 people" and, when the purchase has a limit, "within the $81,600 limit" and the band; the Eligibility track is marked complete

## Outcomes

- **Income over the limit:** the API refuses (`over_income_limit`) and an error toast shows "The verified income of $90,000 is over the 80% of AMI limit of $81,600 for a household of 3 (limits effective Apr 1, 2026)." Nothing is recorded. If the documents are right, the household can't buy this home: staff mark the purchase Failed from Exception Actions, and the home's next selection starts.
- **Household size below 1, or income below zero:** the browser stops the form (the fields have minimums); a request that gets past it is refused by the API, and the error toast shows the message. Nothing is recorded.
- **No limit:** income and household size are recorded and not checked. When a schedule was in effect at the selection's start, the band is still recorded.
- **No schedule** (no schedule in effect at the selection's start, or the purchase didn't start from a selection): no band is recorded, and the funder report shows "Not recorded".
