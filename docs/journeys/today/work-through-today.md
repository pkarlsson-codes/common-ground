# Work Through Today

1. User opens the app at its root, or clicks **Today** at the top of the sidebar
2. The header shows today's date in the trust's time zone and how many things need attention
3. Items are grouped into sections in work order: Applicants, Selections, Purchases, Stewardship, Records. A section with nothing waiting is hidden
4. Within a section, items run overdue first, then due soonest, then waiting longest. Each row shows what's waiting, the record (household, home or document), one line of detail and a due chip: red when overdue, amber when due within a week, plain otherwise. A row with no due date shows how long it has waited
5. What appears:
   - **Applicants**: applications waiting for a decision; households with an open application that are missing readiness steps; prequalifications expiring within 30 days
   - **Selections**: open home selections; available homes with no selection started. Income limits: when a home has an income limit and the limits in effect today have no level for its percent (or no limits are in effect), one item "Income limits missing" whose detail names the missing percents and the homes whose selections can't start (up to 3, then "and N more"). When the limits in effect are more than 12 months old and no later schedule is entered, one item "Income limits out of date", waiting since they turned 12 months old, since HUD publishes new limits every year. Neither appears when no home has an income limit
   - **Purchases**: every open purchase: stalled (due by its resolution target), closing within 14 days, or waiting on the steps it hasn't recorded
   - **Stewardship**: active leases with a late fee notice or notice of default due (see [Send Lease Fee Notices](../stewardship/send-lease-fee-notices.md)); the detail gives the amount due and days overdue. Also active leases whose notice of default wasn't cured by its deadline, due the day after it; the detail gives the deadline and what is still to pay of the amount owed. The item goes once the amount is paid or the lease ends, and is left out while the lease has an unresolved foreclosure prevention case (it comes back if the case is resolved and the default is still not cured). Also mortgage requests waiting for a decision, due on their expected closing date; the detail gives the purpose, lender and principal and the business days to closing. And approved mortgage loans whose expected closing date has passed with no closing recorded, due the day after it; they go once the loan is recorded closed or the request is withdrawn. And active leases with no resale in progress whose yearly resale price statement is due, due a year after the last statement (or the lease start when none was sent); the item goes once a statement is recorded (see [Send Resale Price Statements](../stewardship/send-resale-price-statements.md)). Also active leases whose insurance cover ends within 30 days, has ended, or has no certificate on file, due on the policy end date (or the lease start with none); the detail says "Policy ends {date}", "Policy ended {date}" or "No certificate on file". And active leases whose current insurance certificate falls short of the lease, due on the day it was received; the detail names each shortfall
   - **Records**: documents expiring within 30 days, or expired
6. Each row offers the actions that fix it, one button per fix:
   - **Application to decide**: Approve, Deny
   - **Readiness incomplete**: Record housing counseling, Record CLT orientation, Record prequalification (only the missing steps)
   - **Prequalification expiring**: Record prequalification
   - **Home available**: Start selection
   - **Selection open**: Choose household
   - **Income limits missing** or **out of date**: Enter limits, which opens the Income Limits page with the Enter Income Limits panel open; clicking the row opens the page
   - **Purchase waiting or stalled**: one button per step not yet recorded (Record eligibility, appraisal, financing, inspection, title, lease signing), or Schedule closing once every step is in; **Closing coming up**: Record closing
   - **Lease fee notice due**: Record notice, which opens the lease, where the letter is printed and the notice recorded
   - **Lease default not cured**: Review default, which opens the lease. Ending the lease (Art 12.4) or working with the homeowner is staff's decision; nothing in the app ends it automatically. To work with the homeowner, staff open a foreclosure prevention case from the lease (see [Open Foreclosure Prevention Case from a Lease Default](../ground-leases/open-foreclosure-prevention-case-from-default.md))
   - **Mortgage request to decide**: Decide, or Add appraisal while the request has no appraisal; both open the request (see [View Mortgage Request](../ground-leases/view-mortgage-request.md))
   - **Improvement request to decide**: Decide, which opens the request (see [View Improvement Request](../ground-leases/view-improvement-request.md)). Due two weeks after it was received; red once passed
   - **Mortgage loan closing to confirm**: Confirm closing, which opens Record Loan Closed (see [Record Mortgage Loan Closed](../ground-leases/record-mortgage-loan-closed.md)); clicking the row opens the lease
   - **Resale price statement due**: Prepare statement, which opens the Resale Price Statement panel over Today (see [Prepare Resale Price Statement](../ground-leases/prepare-resale-price-statement.md)); clicking the row opens the lease
   - **Insurance certificate due**: Record certificate, which opens the Record Insurance Certificate panel over Today (see [Record Insurance Certificate](../ground-leases/record-insurance-certificate.md)); clicking the row opens the lease
   - **Insurance short of the lease**: none; the row opens the lease, where staff record an amended certificate or change the requirements
   - **Document expiring**: none; the row itself opens the document
7. An action opens its panel over Today; on success Today refreshes and the item leaves or changes. Start selection, Choose household and the purchase steps open the record's detail for now, where the action is
8. Clicking anywhere else on a row opens the record's detail
9. With a focus other than Everything, only the focus's areas and Records are shown, with a note saying how many items are hidden; the **My areas / All areas** toggle shows the rest.
10. Sidebar items show how many items are waiting on their page. The Today item shows the count the page shows under My areas (every item with Everything). See [Choose a Focus](../navigation/choose-a-focus.md)
11. An item leaves Today once the record no longer needs attention (for example, an application is approved or denied)

Continues in: [Approve Application](../applications/approve-application.md)
