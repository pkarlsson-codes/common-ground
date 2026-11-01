# View Household Detail

1. User opens a household from the Households table, or arrives via a link to it
2. Panel opens titled with the primary contact's name, with their email as subtitle
3. A lifecycle stepper at the top shows Intake → Ready → Approved → Waiting → Selected → Purchasing → Homeowner: steps already passed are marked done with their date, the household's current stage is highlighted, and later steps are dimmed
   - Intake is current from registration until housing counseling, CLT orientation and a mortgage prequalification have all been recorded; Ready is current from then until an application is approved
   - Approval marks Approved done and makes Waiting current (shown from the approved date); selection for a home marks Selected done and makes Purchasing current; closing the purchase makes Homeowner current
   - A purchase that fails or is withdrawn returns the household to Waiting with its original approved date, so it keeps its place on the waiting list, and Selected and Purchasing are no longer marked done
   - An application that is denied or withdrawn returns the household to Ready (or Intake, if its readiness is no longer complete)
4. A Next Step card under the stepper says in one sentence what the household is waiting on, with one primary button:
   - Intake: which readiness items are still missing, with "Record" for the first one
   - Ready: "Start Application", or "Open Application" while one is awaiting a decision; a failed start shows the error on the card
   - Waiting: since when it has been on the waiting list, with "Open Waiting List"
   - Purchasing: the home and the purchase's current step, with "Open Purchase"
   - Homeowner: the home and since when, with "Open Ground Lease"
5. "More actions" opens a menu with the household's other actions: "Start Application" (at Intake, when it has no open application), "Edit Circumstances", and "Record" for each readiness item still missing (or "Update Mortgage Prequalification" once a current one is recorded)
6. Panel shows the household's current Household Size, Registered date, Annual Income and Accessibility Need, with "Edit Circumstances"
7. "Household Members" lists every member with name, email and phone; the primary contact is labelled "Primary Contact". Each member has "Edit", every member but the primary contact has "Remove", and "Add Member" adds one
8. "Buyer Preparation Checklist" shows Housing Counseling, CLT Orientation and Mortgage Prequalification as done or pending; pending items offer "Record", and Mortgage Prequalification offers "Update" once recorded
9. A Connected Records section links to the household's applications (with phase and submitted date), the homes it was selected for, its purchases (with home and phase), and its ground lease, each showing the record's status

Continues in: [Start Application](start-application.md), [Edit Household](edit-household.md), [Record Housing Counseling](record-housing-counseling.md), [Record CLT Orientation](record-clt-orientation.md), [Record Mortgage Prequalification](record-mortgage-prequalification.md)
Continues in: [View Application Detail](../applications/view-application-detail.md), [View Waiting List](../waiting-list/view-waiting-list.md)
Continues in: [View Home Detail](../homes/view-home-detail.md), [View Purchase Workflow Detail](../purchase-workflows/view-purchase-workflow-detail.md), [View Ground Lease Detail](../ground-leases/view-ground-lease-detail.md)
