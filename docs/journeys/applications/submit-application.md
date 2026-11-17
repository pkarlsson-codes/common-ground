# Submit Application

1. User navigates to Applications
2. User clicks "New Application"
3. "Submit Application" panel opens with "New household" selected

New household:

4. The form starts with one empty household member, marked as primary contact
5. For each household member, user enters Full Name, Email, and Phone (all required); "Add Member" adds another household member and "Remove" removes one (at least one member always remains)
6. User picks exactly one household member as "Primary contact"; Household Size is shown as the number of household members
7. User enters Annual Income and optionally checks "Accessibility accommodations required"
8. User clicks "Submit Application" (disabled until every household member has Full Name, Email, and Phone); invalid phone numbers show an inline error
9. When an email typed for a member (compared ignoring case and surrounding spaces) matches any member of an existing household, a warning under the field reads "A household with this email exists: <primary contact name>." with "Use the existing household"; clicking it switches to "Existing household" with that household selected. The warning doesn't block submitting, because two people can share an email
10. The household is registered, then an application is submitted for it, capturing household size, income, and accessibility as declared at submission

Existing household (e.g. one returning after a withdrawn or denied application):

4. User selects "Existing household" and types in "Search by member name or email"; up to five matching households are listed with primary contact, email and size, matching any member of the household
5. User clicks "Select" on a household; "Selected Household" shows its members, size, income and accessibility ("Change" goes back to the search)
6. If the household already has an open (Submitted, Approved or Selected) application, "This household already has an open application." is shown with "Open Active Application", and "Submit Application" stays disabled
7. Otherwise user clicks "Submit Application"; an application is submitted for the household, capturing its current size, income and accessibility. To change those first, edit the household from Households

Either way:

- Toast: "Application submitted (<id>...)"
- Panel closes, new application appears in the Applications table in the Submitted phase, showing the primary contact's name and email

The API rejects an application for a household that already has an open application; the panel prevents this for an existing household. Applying from the household detail works the same way: see [Start Application](../households/start-application.md).
