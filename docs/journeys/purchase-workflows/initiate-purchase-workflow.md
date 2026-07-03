# Initiate Purchase Workflow

1. User navigates to Purchase Workflows
2. User clicks "Initiate Workflow"
3. "Initiate Purchase Workflow" panel opens with Purchase Price defaulted to 150000
4. User searches for and selects a Lease Template (only Active templates are offered)
5. User searches for and selects a Land Parcel
6. User searches for and selects a Home on that parcel (disabled until a parcel is selected; changing the parcel clears the home)
7. User enters Buyer Full Name and Buyer Email (required), and optionally Buyer Phone in E.164 format (e.g. +11231231234)
8. User optionally adjusts Purchase Price
9. User clicks "Initiate Workflow" (disabled until a Lease Template, Land Parcel, and Home are all selected)
10. Toast: "Purchase workflow initiated"
11. Panel closes, new workflow appears in the Purchase Workflows table in the Initiated phase

The ground lease itself is not created yet — it is created automatically when the purchase closes. See [Record Purchase Closed](record-purchase-closed.md).
