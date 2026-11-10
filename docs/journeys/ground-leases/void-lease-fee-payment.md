# Void Lease Fee Payment

For a payment recorded by mistake: wrong lease, wrong amount, or a check that bounced. To correct an amount, void the payment and record it again.

1. User opens a ground lease (see [View Ground Lease Detail](view-ground-lease-detail.md)) and clicks "Void" on a payment in the Lease Fees section
2. "Void Payment" panel opens with the payment's date, amount and method as subtitle, and says the payment will no longer count toward the balance
3. User enters a Reason (required)
4. User clicks "Void Payment" (red, disabled until a reason is entered), or "Cancel"
5. A confirmation asks "Void this payment? This can't be undone. To correct an amount, record the payment again." User clicks "Void Payment" to go ahead, or "Cancel" to return to the panel
6. Toast: "Payment voided"
7. The payment stays in the history marked Voided with its reason, and no longer has a Void action. The balance, oldest unpaid month and days overdue update
