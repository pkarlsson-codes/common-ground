# Create Ground Lease

For recording a lease that didn't come from a purchase, such as one signed before the trust used CommonGround. A lease created this way has the same effects as one from a purchase closing; only its history differs (no purchase, no application).

1. User navigates to Ground Leases
2. User clicks "Create Lease"
3. "Create Ground Lease" panel opens (subtitle: "Register a new community land trust ground lease")
4. User searches and selects a Land Parcel
5. User searches and selects a Home on that parcel (disabled until a parcel is chosen)
6. User enters the Homeowner's Full Name and Email (both required) and optionally a Phone number
7. User enters Lease Start Date (default today; may be years back for a lease that predates CommonGround) Term (years, default 99), Option Window (days the trust has to buy the home after the homeowner gives notice to sell, default 60), and the monthly Land Use Fee and Repair Reserve Fee ($, both required, zero or more, both default 0; the form shows their sum as the Monthly Lease Fee, due on the 1st), and the late interest terms as on a template (see [Create Ground Lease Template](../ground-lease-templates/create-ground-lease-template.md)), copied from the signed lease. Fees are charged on the 1st of each month from the first 1st on or after the day fees are tracked from
8. The form also asks for Fees Tracked From (not before the start date; months before it aren't charged) and Owed on That Day ($, zero or more: fees and interest still unpaid from before; enter 0 if paid up). Both are required and have no default, so the form never guesses what the homeowner owes. The amount owed is due on the tracked-from day, so a lease with a balance shows it on Today and in late notices from then on. Payments received before that day can't be recorded; they belong in the amount owed. A lease that has just started is entered with Fees Tracked From set to its start date and 0 owed
9. User enters Covenant Terms (required), Base Price (what the homeowner paid for the home) and Original Appraised Value (the home's appraised value at that purchase). Both are required and above zero
10. User selects a Resale Formula: Appraisal Based, Index Based, or Fixed Percentage (default Appraisal Based)
11. If Appraisal Based, user enters Appreciation Share (decimal fraction, e.g. 0.25 = 25%, default 0.25)
12. If Index Based, user selects Index Type (CPI, AMI, or Custom, default CPI) and enters Baseline Index Value (default 100)
13. If Fixed Percentage, user enters Annual Percentage (decimal fraction, e.g. 0.01 = 1%, default 0.01)
14. User optionally checks "Set improvements cap" and enters an Improvements Cap ($, default 25000 when first checked)
15. User enters Debt Cap (% of the purchase option price, whole number, 1 to 200, required, no default; help text "The most total mortgage debt may be when the trust permits a new loan (Art 8.3). Commonly 80–100%.")
16. User optionally enters Replacement Value ($, the home's insured replacement value agreed with the homeowner) and Liability per Occurrence ($). Help text "Insurance the homeowner must carry (Art 9.4). Left empty, the app doesn't check that amount."
17. User clicks "Create Lease" (disabled until Land Parcel and Home are both selected)
18. If the home already has an active lease, an open home selection, or an open purchase, the panel stays open with an error saying why, and nothing is created
19. Otherwise: toast "Ground lease created", the panel closes, and the new lease appears at the top of the Ground Leases table (newest first), titled with the homeowner's name
20. Shortly after:
    - Stewardship opens a case for the lease, dated the day the lease was recorded (not its start date)
    - The home shows Leased on Homes, leased since the lease start date, and no longer appears on Today as an available home

Continues in: [View Ground Lease Detail](view-ground-lease-detail.md)
