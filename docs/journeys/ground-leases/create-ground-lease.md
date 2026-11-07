# Create Ground Lease

For recording a lease that didn't come from a purchase, such as one signed before the trust used CommonGround. A lease created this way has the same effects as one from a purchase closing; only its history differs (no purchase, no application).

1. User navigates to Ground Leases
2. User clicks "Create Lease"
3. "Create Ground Lease" panel opens (subtitle: "Register a new community land trust ground lease")
4. User searches and selects a Land Parcel
5. User searches and selects a Home on that parcel (disabled until a parcel is chosen)
6. User enters the Homeowner's Full Name and Email (both required) and optionally a Phone number
7. User enters Lease Start Date (default today; may be years back for a lease that predates CommonGround) Term (years, default 99) and Option Window (days the trust has to buy the home after the homeowner gives notice to sell, default 60)
8. User enters Covenant Terms (required), Base Price (what the homeowner paid for the home) and Original Appraised Value (the home's appraised value at that purchase). Both are required and above zero
9. User selects a Resale Formula: Appraisal Based, Index Based, or Fixed Percentage (default Appraisal Based)
10. If Appraisal Based, user enters Appreciation Share (decimal fraction, e.g. 0.25 = 25%, default 0.25)
11. If Index Based, user selects Index Type (CPI, AMI, or Custom, default CPI) and enters Baseline Index Value (default 100)
12. If Fixed Percentage, user enters Annual Percentage (decimal fraction, e.g. 0.01 = 1%, default 0.01)
13. User optionally checks "Set improvements cap" and enters an Improvements Cap ($, default 25000 when first checked)
14. User clicks "Create Lease" (disabled until Land Parcel and Home are both selected)
15. If the home already has an active lease, an open home selection, or an open purchase, the panel stays open with an error saying why, and nothing is created
16. Otherwise: toast "Ground lease created", the panel closes, and the new lease appears at the top of the Ground Leases table (newest first), titled with the homeowner's name
17. Shortly after:
    - Stewardship opens a case for the lease, dated the day the lease was recorded (not its start date)
    - The home shows Leased on Homes, leased since the lease start date, and no longer appears on Today as an available home

Continues in: [View Ground Lease Detail](view-ground-lease-detail.md)
