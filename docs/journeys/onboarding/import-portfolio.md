# Import the Portfolio

For a trust that already has homes and leases when it starts using CommonGround. One spreadsheet row per home.

1. User clicks **Import portfolio** in the sidebar, under Settings
2. User clicks "Download template" and gets `portfolio-import-template.csv`: the column headers and one example row. The page lists each column, whether it is required, and its allowed values
3. User fills one row per home in a spreadsheet program and saves it as CSV. Rows with the same Parcel Number share one land parcel. A row with no Homeowner Name is a home without a lease. Lease Template names one of the trust's active ground lease templates, which supplies the term, option window, covenant, resale formula and interest terms
4. User chooses the file and clicks "Check File". Choosing a different file clears the last check
5. If any row has errors, a table lists each one: row number, column, and the error ("Row 7, Bedrooms: enter a whole number."). Nothing can be imported until the file is fixed and checked again
6. If no row has errors, the page says how many land parcels, homes and ground leases will be created, and how many already exist and will be skipped. "Import" is disabled when there is nothing to create
7. User clicks "Import". A toast says "Imported {n} homes", and the page shows links to Land Parcels, Homes and Ground Leases
8. If the import stops part way (a server error), the page says which row it stopped at. Choosing the same file and importing again skips everything already created

Notes:
- Fees Tracked From defaults to the 1st of this month and Amount Owed Then to zero, so leases signed years ago don't fill Today with late notices for months before the trust used CommonGround. See [Create Ground Lease](../ground-leases/create-ground-lease.md).
- Income Limit is the home's limit as a percent of AMI, e.g. `80`, or `None` for a home sold at market rate. Empty means 80, as on [Register Home](../homes/register-home.md).
- Dates may be written `2005-06-01` or `6/1/2005`. Amounts may include `$` and `,`.

Continues in: [Work through Today](../today/work-through-today.md)
