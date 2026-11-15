# Open Heir Transfer

Art 10.3: when a homeowner dies, the executor tells the trust within 90 days, and the trust consents to a transfer to a qualifying heir. Staff open a transfer when they hear of the death.

1. User opens a ground lease (see [View Ground Lease Detail](view-ground-lease-detail.md)) and clicks "Open Heir Transfer" in its Heir Transfer section (Active leases with no open transfer only)
2. "Open Heir Transfer" panel opens, showing the homeowner and the home
3. User enters Date of Death (required, not in the future), Notice Received On (default today, not before the date of death, not in the future), Notified By (required; the executor's name), Contact (optional) and Notes (optional)
4. When Notice Received On is more than 90 days after the date of death, the panel says "Late notice: Art 10.3 asks for notice within 90 days." The user can still open the transfer
5. User clicks "Open Transfer", or "Cancel"
6. Toast: "Heir transfer opened". The panel closes. The Heir Transfer section shows the transfer as Open, with "Notice was late" when it was

## Outcomes

- **The lease already has an open transfer:** the button is not shown; a request is refused (`already_open`).
- **Date of death in the future, or notice before the death:** the browser stops the form; the API refuses and the error toast shows the message.

Continues in: [Record Heir](record-heir.md)
