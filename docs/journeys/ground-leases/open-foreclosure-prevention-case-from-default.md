# Open Foreclosure Prevention Case from a Lease Default

Staff open a foreclosure prevention case when the homeowner hasn't paid the amount a notice of default said was owed, and the trust chooses to work with the homeowner instead of ending the lease (Art 12.1, 12.4; Ch 25).

1. User reaches the ground lease's detail panel: from Today's "Lease default not cured" item (Review default), from a row on the Stewardship page's Lease Fees tab, or from the Ground Leases page
2. In the Lease Fees section, the notice of default box shows where the notice stands. While it is unpaid (cure period running, or deadline passed and still unpaid) and the lease has no unresolved foreclosure prevention case, the box has an "Open Prevention Case" button
3. User clicks "Open Prevention Case". No form: the case takes the lease, today's date and the notice figures from the lease fee ledger
4. Toast: "Foreclosure prevention case opened"
5. The box now says "Foreclosure prevention case open since {date}" with a "View Case" link, which opens the case on the Stewardship page, Foreclosure Prevention tab. The line stays while the case is unresolved, also after the notice is paid
6. The case has trigger "Lease fee arrears", status Opened, and a Lease Fee Arrears section: "Notice of default sent {date} for $X. Cure by {date}. $Y still to pay when the case opened."
7. Today no longer lists "Lease default not cured" for the lease while the case is unresolved
8. If the API refuses (the notice was paid meanwhile, or another case was opened meanwhile), its message is shown in the box: "This lease has no unpaid notice of default." or "This lease already has an open foreclosure prevention case."

Continues in: [View Foreclosure Prevention Case Detail](../stewardship/view-foreclosure-prevention-case-detail.md)
Continues in: [Make Counseling Referral](../stewardship/make-counseling-referral.md)
