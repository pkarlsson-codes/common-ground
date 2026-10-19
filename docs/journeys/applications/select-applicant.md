# Select Applicant From Home

1. User navigates to Homes
2. User clicks a row for a home with status Available to open the detail panel
3. User clicks "Select Applicant" next to the home's status (only shown while the home is Available)
4. "Select Applicant" panel opens with Selected Date defaulted to today; the home is fixed to the one just opened
5. User picks an Applicant from a combobox listing Approved-phase homeowner applications whose household has completed housing counseling and CLT orientation and holds a mortgage prequalification valid today, by primary contact name and email; typing filters the list locally (income/AMI eligibility filtering against the home is not yet implemented)
6. User searches for and selects an active Ground Lease Template, and enters a Purchase Price
7. User optionally adjusts Selected Date
8. User clicks "Select Applicant" (disabled until Applicant, Ground Lease Template, Purchase Price, and Selected Date are all set)
9. Toast: "Applicant selected"
10. Panel closes; the application moves to the Selected phase, the home moves to the UnderContract status, and a purchase workflow is automatically created linking back to the application

The selection is also rejected if the household's readiness is incomplete or its prequalification expires before the Selected Date.

If another applicant is already mid-selection for the same home, the second attempt is rejected with a conflict error instead of creating a duplicate workflow.

Continues in: [View Application Detail](view-application-detail.md)
Continues in: [View Purchase Workflow Detail](../purchase-workflows/view-purchase-workflow-detail.md)
