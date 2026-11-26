# Record Buyback Closed

Simplified for the first slice. The system records that the closing happened; the buyback's own steps (funds, inspection, repairs, title, scheduling) and price deductions such as appliances are not tracked yet.

1. User opens a resale whose option is exercised (see [View Resale Detail](view-resale-detail.md)) and clicks "Record Closing"
2. "Record Closing" panel opens, showing the Purchase Option Price
3. User enters Closing Date (default today, not in the future, not before the Exercised Date), Purchase Price Paid (default the Purchase Option Price, required, at most the Purchase Option Price) and optional Closing Notes
   - Fees Paid from Proceeds (default the lease's Total Owed on the Closing Date, zero or more). Above zero, a lease fee payment with method "Sale proceeds" is recorded on the lease before it ends
   - User chooses What Happens Next: "Offer to the waiting list" (default) or "Hold for now"
   - Choosing "Hold for now" adds Hold Reason (required, free text)
4. User clicks "Record Closing", or "Cancel"
5. Toast: "Buyback closed"
6. The resale becomes Closed and leaves the open list on Resales. The Next Step card says the trust bought the home on the closing date
7. Shortly after:
    - The homeowner's ground lease becomes Inactive, ended on the closing date, and its Resale section shows the closed resale
    - With "Offer to the waiting list": the home shows Available on Homes, appears on Today as a home available again after the trust bought it back, and its Next Step card offers "Choose a Household". Households chosen for it before the buyback don't stop a new choice
    - With "Hold for now": the home shows Held by Trust on Homes and does not appear on Today. Its Next Step card shows the Hold Reason and offers "Release for Sale"

Continues in: [Release Home for Sale](../homes/release-home-for-sale.md)
Continues in: [View Home Selection](../home-selections/view-home-selection.md)
Continues in: [View Ground Lease Detail](../ground-leases/view-ground-lease-detail.md)
