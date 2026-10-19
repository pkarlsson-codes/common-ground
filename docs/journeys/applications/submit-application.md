# Submit Application

1. User navigates to Applications
2. User clicks "New Application"
3. "Submit Application" panel opens with one empty household member, marked as primary contact
4. For each household member, user enters Full Name, Email, and Phone (all required); "Add Member" adds another member and "Remove" removes one (at least one member always remains)
5. User picks exactly one member as "Primary contact"; Household Size is shown as the number of members
6. User enters Annual Income and optionally checks "Accessibility accommodations required"
7. User clicks "Submit Application" (disabled until every member has Full Name, Email, and Phone); invalid phone numbers show an inline error
8. The household is registered, then an application is submitted for it, capturing household size, income, and accessibility as declared at submission
9. Toast: "Application submitted (<id>...)"
10. Panel closes, new application appears in the Applications table in the Submitted phase, showing the primary contact's name and email
