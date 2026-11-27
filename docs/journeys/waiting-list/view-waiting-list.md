# View Waiting List

1. User navigates to Waiting List
2. Header shows how many households are waiting
3. Table lists every application in the Approved phase, longest waiting first: Primary Contact, Size, Approved (date), Days Waiting, Prequalification Expires, Passed Over, and Readiness
4. Passed Over counts the times another household was chosen for a home while this household could have been. It follows the household, so it carries over if the household withdraws or is removed and applies again. It is updated just after a household is chosen, so it can briefly lag behind
5. Readiness shows "Qualification to confirm" when the household's income or size changed after approval and staff haven't [confirmed it still qualifies](../applications/confirm-qualification.md); it keeps its place in line but can't be chosen until then. Otherwise Readiness shows "Needs re-verification" when the household's mortgage prequalification has expired (whether or not it has since been cleared), "Ready" when housing counseling and CLT orientation are recorded and the prequalification is still valid, and "Not ready" otherwise
6. User types in "Search by member name or email"; the table filters as they type, matching any member of the household
7. Each row is an application, shown with its household's details. User clicks a row to open the application's detail, where staff can [confirm qualification](../applications/confirm-qualification.md) or [remove it from the waiting list](../applications/remove-from-waiting-list.md). "Open Household" in that panel leads to the household, where readiness is recorded and the prequalification updated

Households join the waiting list when their application is approved ([Approve Application](../applications/approve-application.md)) and leave it when the household is chosen for a home, withdrawn, or [removed from the waiting list](../applications/remove-from-waiting-list.md) by staff (no longer eligible, no response, false information, or another reason). A household whose purchase fails or is withdrawn returns to the list with its original approved date, so it keeps its place. Every household on the list is compared against a home on its [selection page](../home-selections/view-home-selection.md), and the whole list is recorded when one is chosen.

Continues in: [View Application Detail](../applications/view-application-detail.md), [View Household Detail](../households/view-household-detail.md)
