# Complete Heir Transfer

The trust consents once the heir and the heir's lawyer have signed the Letter of Agreement and the Attorney's Acknowledgment (Art 10.3, Art 1). The lease then names the heir as homeowner.

1. In the Heir Transfer panel of a transfer with an heir (see [View Heir Transfer](view-heir-transfer.md)), user clicks "Complete Transfer"
2. "Complete Heir Transfer" panel opens, showing the heir, the relationship and the current homeowner it replaces
3. User enters Transfer Date (default today, not before the date of death, not in the future) and optional Notes
4. User chooses the signed Letter of Agreement and the signed Attorney's Acknowledgment (both required; PDF or image)
5. User clicks "Complete Transfer", or "Cancel". The files upload to Documents, attached to the lease with the roles `heir-letter-of-agreement` and `heir-attorney-acknowledgement`; the API then records the completion
6. Toast: "Transfer completed". The lease shows the heir as homeowner (name, email, phone). Previous Homeowners lists the deceased with the transfer date. Lease fees, mortgages, insurance, improvements and the resale terms are unchanged
7. "Print Letter" opens a print page in a new tab: the date, the heir's name, the home, and the sentence that the trust consents to the transfer under Art 10.3, with the signed letters attached to the lease

## Outcomes

- **A file is missing:** the browser stops the form; a request without one is refused (`letters_required`).
- **The lease has an open resale:** refused (`open_resale`). Staff close the transfer or finish the resale.
- **Upload fails:** the panel stays open with the error and nothing is recorded. Trying again reuses files already uploaded, so Documents gets no second copy.
