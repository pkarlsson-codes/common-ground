# View Funder Report

Once a year, staff report to each funder what the trust holds and whom it sold to. The report is worked out from the records each time it opens.

1. User navigates to Funder Report (sidebar, Records)
2. From and To default to last calendar year (Jan 1 to Dec 31). User can change either date, for a fiscal year
3. The page shows four sections for the period:
   - **Portfolio on {To}:** homes registered by then (or under a ground lease that had started by then), homes under a ground lease on that day, and homes held by the trust or for sale
   - **Sales:** one row per ground lease that starts in the period: home, start date, price, appraised value, affordability given (appraised value minus price), household size, verified income and income band ("Up to 50% of AMI"). Leases with no purchase in the app show "Not recorded" for size, income and band
   - **Households by income band:** the number of sales in each band, and "Not recorded"
   - **Resales:** one row per buyback that closed in the period: home, the seller's original price, appraised value, purchase option price, price paid, affordability kept (appraised value minus price paid) and the seller's gain (price paid minus original price)
4. Totals under Sales and Resales add up affordability given and kept
5. User clicks a Sales row to open its ground lease, or a Resales row to open its resale
6. User clicks "Download CSV" on Sales or Resales; the browser saves `funder-report-sales-{from}-{to}.csv` or `funder-report-resales-{from}-{to}.csv` with the table's columns

## Outcomes

- **No sales or resales in the period:** the section shows "No sales in this period" or "No resales in this period", and its Download CSV button is disabled.
- **To before From:** the page shows "To must be on or after From" and doesn't load the report.

Continues in: [view ground lease detail](../ground-leases/view-ground-lease-detail.md), [view resale detail](../resales/view-resale-detail.md)
