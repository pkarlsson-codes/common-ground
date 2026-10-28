# View Expiring Documents

1. User navigates to Documents
2. If any documents have expired or expire within 90 days, a banner shows "{n} document(s) expired or expiring within 90 days" with a "Show expiring" button
3. User clicks "Show expiring"; the library view is replaced by those documents, soonest expiry first (button label changes to "Show all")
4. Each row shows title, "Expires {date}" (or "Expired {date}" once past), and file name; rows that are expired or expire within 30 days are highlighted amber
5. User clicks "Download" on a row to download its current version in a new tab, or clicks the row to open its detail panel ([Edit document details](edit-document-details.md))
6. User pages through with "Prev"/"Next" when there are more than 20
7. User clicks "Show all" to return to the filterable Document Library view
