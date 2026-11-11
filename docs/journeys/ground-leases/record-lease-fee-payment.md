# Record Lease Fee Payment

For when the trust receives a homeowner's lease fee, by check, transfer or any other way.

1. User opens a ground lease (see [View Ground Lease Detail](view-ground-lease-detail.md)), from Ground Leases or from a row on the Lease Fees tab (see [View Lease Fees](../stewardship/view-lease-fees.md)), and clicks "Record Payment" in the Lease Fees section
2. "Record Payment" panel opens; its subtitle is the homeowner's name and the current balance ("$120.00 due", "$40.00 credit" or "Paid up")
3. User enters Received Date (default today, not in the future, not before the day the lease's fees are tracked from, which is the lease start date unless the lease was created with a later one), Amount (required, above zero, prefilled with the balance due when there is one, otherwise the monthly fee), Method (Check, Bank Transfer, Cash, Money Order, Card or Other; default Check) and an optional Reference (for example the check number)
4. User clicks "Record Payment", or "Cancel"
5. Toast: "Payment recorded"
6. The payment appears at the top of the lease's payment history, and the balance, oldest unpaid month and days overdue update
7. If the balance is now zero or less, the lease leaves the Lease Fees tab's default list

A lease that has ended still takes payments, so the trust can collect what was owed before it ended.

Continues in: [Void Lease Fee Payment](void-lease-fee-payment.md)
