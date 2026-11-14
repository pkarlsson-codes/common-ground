# Record Disclosure Acknowledged

Art 1 has the trust give the buyer a disclosure letter before the buyer commits: the resale formula, the lease terms, what the buyer gives up. The buyer signs an acknowledgment. The app keeps the signed copy and the date. The letter itself is the trust's own text.

1. User navigates to Purchases
2. User clicks a row for a non-terminal workflow to open the detail panel
3. In Purchase Tracks, user expands the Disclosure track (only expandable while pending)
4. User enters Acknowledged Date (defaulted to today) and optional Notes
5. User chooses the signed acknowledgment file (required)
6. User clicks "Record Disclosure"
7. The file uploads to Documents, attached to the purchase with the role `disclosure-acknowledgement`; the API then records the track
8. Toast: "Disclosure recorded"
9. Track collapses and shows "Acknowledged {date}" and is marked complete; a Closing Documents section lists a link to the signed file

## Outcomes

- **No file chosen:** the browser stops the form. Nothing is recorded.
- **Acknowledged date in the future:** the API refuses and an error toast shows the message. The uploaded file stays in Documents. Nothing is recorded on the purchase.
- **Already recorded:** the track is not expandable. A mistake is corrected by the later roadmap item for correcting tracks.
- **Closing:** Schedule Closing needs this track complete, with the other six tracks.
