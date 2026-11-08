# Close an Overlay

Panels (slide-overs) and confirmation dialogs are overlays. They can stack: a "Record Housing Counseling" panel opens over a household panel, and a "Mark as Failed" dialog opens over a purchase panel.

1. User has two or more overlays open
2. User presses Escape
3. Only the top overlay closes; the ones under it stay open
4. If the top overlay is a panel with typed input, a "Discard changes?" dialog opens first ("What you typed will be lost.")
5. User clicks "Discard" to close the panel and lose the input, or "Cancel" (or presses Escape again) to keep editing

Close and Cancel in a panel's header or footer and a click on the backdrop do the same as Escape.

Panels that ask before discarding: Record Housing Counseling, Record CLT Orientation, Record Mortgage Prequalification, Deny Application, Withdraw Application, and the purchase panel when a Fail or Withdrawal Reason is typed. A panel whose form has not changed from its defaults closes at once.
