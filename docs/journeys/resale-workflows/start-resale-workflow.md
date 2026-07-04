# Start Resale Workflow

1. User navigates to Resale Workflows, showing active ground leases (Homeowner, Address, Formula, Status)
2. User clicks "Start Resale" on a lease row
3. "Resale" panel opens (titled with the homeowner's name), Step 1 of 3: Intent
4. Panel shows Homeowner, Lease Start Date, Formula, and Ground Lease ID; user enters Homeowner Notice Date (defaults to today)
5. User clicks "Calculate Resale Price →"
6. Step 2 of 3: Estimate — a notice reminds the user the original purchase price must be sourced from paper records or closing documents
7. User enters Original Purchase Price, Current Appraisal Value, Original Appraisal Value, Ownership Years (required), and optionally Approved Improvements and Current Index Value
8. User clicks "Initiate Purchase Workflow →"
9. Max resale price is calculated and shown (e.g. "Max resale: $123,000"); user can click "Back" to revise inputs
10. Step 3 of 3: Initiate — Ground Lease is shown read-only; Purchase Price is pre-filled with the calculated max resale price (editable, with "Formula max" shown as a hint)
11. User enters Buyer Full Name and Buyer Email (required), and optionally Buyer Phone (formatted as-you-type, e.g. (202) 555-1234)
12. User clicks "Initiate Workflow"
13. Toast: "Purchase workflow initiated"
14. Panel closes, user is navigated to Purchase Workflows with the new workflow's detail panel open

The new workflow reuses the lease template, land parcel, and home of the ground lease being resold. If the lease was created directly (without a lease template), initiating fails with an error explaining that a purchase workflow cannot be started from it.

Continues in: [View Purchase Workflow Detail](../purchase-workflows/view-purchase-workflow-detail.md)
