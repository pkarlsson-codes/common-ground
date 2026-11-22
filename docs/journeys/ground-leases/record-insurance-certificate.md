# Record Insurance Certificate

The homeowner must keep the home insured for its replacement value and carry liability insurance naming the trust as additional insured, and send a certificate every year (Art 9.4). Staff record each certificate on the lease, and the app checks it against the lease's requirements.

1. User opens a ground lease (see [View Ground Lease Detail](view-ground-lease-detail.md)) and clicks "Record Certificate" in its Insurance section, or clicks "Record certificate" on an Insurance lapsed, Insurance ending soon or Insurance short of the lease item on Today, which opens the panel over Today
2. "Record Insurance Certificate" panel opens, showing the homeowner, the home and the lease's requirements (Replacement Value and Liability per Occurrence, each "Not set" when absent)
3. User enters Insurer (required), Policy Number (required), Policy Start and Policy End (required; end after start; End fills with the same date a year after Start until the user changes it), Dwelling Coverage ($, required, zero or more), Liability per Occurrence ($, required, zero or more), checks "Trust named as additional insured" (default unchecked), Received On (default today, not in the future) and optional Notes
4. User optionally adds the certificate file (choose or drop; PDF or image)
5. As the amounts are entered, the panel shows the check: "Meets the lease" in green, or each shortfall in amber ("Dwelling coverage $250,000.00 is below the replacement value $280,000.00", "Liability $100,000.00 is below the $300,000.00 required", "The trust is not named as additional insured"). A requirement that isn't set shows "Replacement value not checked: none set on the lease"
6. User clicks "Record Certificate", or "Cancel". A certificate that falls short can still be recorded: it is what the homeowner sent
7. Toast: "Insurance certificate recorded". The panel closes. The lease's Insurance section shows Covered Through the policy end date and lists the certificate. An Insurance lapsed or Insurance ending soon item for the lease leaves Today when the new policy ends more than 30 days from today. If the certificate falls short, an Insurance short of the lease item shows on Today
8. If the file upload fails, the panel stays open with the error and nothing is recorded. If the upload works but recording fails, trying again reuses the uploaded file, so Documents gets no second copy

Continues in: [View Ground Lease Detail](view-ground-lease-detail.md)
Continues in: [Remove Insurance Certificate](remove-insurance-certificate.md)
