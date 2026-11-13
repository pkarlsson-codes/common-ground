# Open Foreclosure Prevention Case

1. User navigates to Stewardship, Foreclosure Prevention tab
2. User clicks "Open New Case"
3. "Open Foreclosure Prevention Case" panel opens, subtitled "Record a servicer delinquency and open a new case"
4. User searches and selects an active Ground Lease (by homeowner name or email), enters Servicer Name, Delinquent Months (1-120), and confirms Recorded Date (defaulted to today)
5. User clicks "Open Case" (disabled until Ground Lease, Servicer Name, and Delinquent Months are filled)
6. Toast: "Foreclosure prevention case opened"
7. Panel closes, new case appears at the top of the Foreclosure Prevention table with status Opened and its delinquency in the Delinquency column
8. If the API rejects the request, its error is shown above the form and the panel stays open. A lease that already has an unresolved case is refused: "This lease already has an open foreclosure prevention case."

A case for unpaid lease fees is opened from the lease instead: [Open Foreclosure Prevention Case from a Lease Default](../ground-leases/open-foreclosure-prevention-case-from-default.md)
