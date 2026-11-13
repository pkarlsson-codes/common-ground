# Record Lease Fee Notice

For one lease: print its late notice or notice of default and record that it was sent.

1. User opens a ground lease (see [View Ground Lease Detail](view-ground-lease-detail.md)), from Ground Leases, the Lease Fees tab or a Lease fee notice item on Today
2. When a notice is due, the Lease Fees section shows "First Notice due", "Second Notice due" or "Notice of Default due" with "Print Letter" and "Record Notice"
3. User clicks "Print Letter"; the letter opens in a new tab with the print dialog (the same letter as in [Send Lease Fee Notices](../stewardship/send-lease-fee-notices.md)). A notice of default prints with a copy for each current permitted mortgage
4. User clicks "Record Notice". "Record Notice" panel opens; its subtitle is the homeowner's name and the balance
5. User picks Level (First Notice, Second Notice or Notice of Default, default the one due), Date Sent (default today, not in the future, not before the lease start) and Delivery (default First-Class Mail; for a Notice of Default only Certified Mail, Registered Mail or Hand Delivered are offered, default Certified Mail, with the help text "A notice of default must be sent by certified or registered mail, or handed over in person. The cure period runs from the date sent.")
6. User clicks "Record Notice", or "Cancel"
7. Toast: "Notice recorded". The notice appears at the top of the section's notice history with its level, date sent, delivery, the lenders a notice of default was copied to (the lease's current permitted mortgages on the date sent; "Copied to: none" when it had none), and the balance and oldest unpaid month on that date. The "notice due" line goes away until the next notice falls due
8. Recording is refused when the lease owed nothing on the date sent: "This lease had no balance due on that date."
9. A Notice of Default keeps the trust's cure period (see [Set Lease Fee Notice Policy](../trust-policy/set-lease-fee-notice-policy.md)) and the balance on the date sent as the amount owed. The section then shows where it stands (see [View Ground Lease Detail](view-ground-lease-detail.md)). It is refused when sent any other way (Art 14.2): "A notice of default must be handed over in person or sent by certified or registered mail (Art 14.2)." It is also refused while an earlier notice of default's cure period is still running: "The last notice of default's cure period is still running."

A notice can be recorded when none is due, for example to log a letter sent early.
