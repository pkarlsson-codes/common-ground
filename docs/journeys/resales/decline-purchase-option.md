# Decline Purchase Option

For when the trust decides not to buy the home. The homeowner still sells, so the home goes through home selection for a new buyer at the Purchase Option Price. The resale stays open until that buyer's purchase closes.

1. User opens a resale that has an appraisal and no decision (see [View Resale Detail](view-resale-detail.md)) and clicks "Decline to Buy"
2. "Decline to Buy" panel opens with the home's label as subtitle, and says the home will be offered to the waiting list at the Purchase Option Price
3. User enters Declined Date (default today, not in the future, not before the appraisal date) and a Reason (required)
4. User clicks "Decline Option", or "Cancel"
5. Toast: "Purchase option declined"
6. The tracker moves to Buyer. The Next Step card says the trust declined to buy, and offers "Choose a Household"
7. The resale stays on Resales, and the home stays at the Resale stage on Homes

The selection and the purchase that follow work as for a first sale, with these differences:
- Choose a Household is offered on the resale and on the home's Next Step card ([View Home Selection](../home-selections/view-home-selection.md)). A household can be chosen only for a home that is offered, so declining must offer the home to the waiting list
- the Purchase Price when selecting a household is the Purchase Option Price, and cannot be higher ([Select Household by Priority](../home-selections/select-household-by-priority.md))
- the purchase's Appraisal track is prefilled from the resale's appraisal ([Record Appraisal Completed](../purchase-workflows/record-appraisal-completed.md))
- when the purchase closes, the seller's lease ends and the resale closes as Sold ([Record Purchase Closed](../purchase-workflows/record-purchase-closed.md))
- if the purchase fails or is withdrawn, the home is available again for staff to choose another household, and the resale stays open

Continues in: [View Home Selection](../home-selections/view-home-selection.md)
