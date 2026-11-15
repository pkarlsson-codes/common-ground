# Record Heir

1. In the Heir Transfer panel of an Open or Heir recorded transfer (see [View Heir Transfer](view-heir-transfer.md)), user clicks "Record Heir" (or "Change Heir")
2. "Record Heir" panel opens
3. User enters Full Name (required), Email (required: the lease's notices go to it), Phone and Relationship: Spouse, Child, Household Member or Other
4. For Household Member: User enters Lived In Home Since (required, not after the date of death). The panel says "Art 10.3 needs a year in the home before the death" and shows "Under one year" in red when the date is less than a year before it
5. For Other: User enters Verified Annual Income and Household Size (both required) and chooses one or more income documents (pay stubs, tax returns; at least one). The panel shows the home's limit for that household size ("Limit for 3 people: $81,600"), with "Over income limit" when the income is higher, or "No income limit on this home" when it has none
6. User clicks "Record Heir", or "Cancel". The income files upload to Documents, attached to the lease with the role `heir-income-document`; the API then records the heir
7. Toast: "Heir recorded". The transfer shows Heir recorded. Recording again replaces the heir

## Outcomes

- **Household Member under one year:** the API refuses (`household_member_under_one_year`) and the error toast shows the message. If the heir is income-qualified, user picks Other.
- **Other over the income limit:** the API refuses (`over_income_limit`) with the limit, household size and schedule date. Nothing is recorded. The heir may inherit the value but not the home: staff close the transfer (see [Close Heir Transfer](close-heir-transfer.md)).
- **The home has an income limit but no schedule in effect on the notice date has its level:** the API refuses (`income_limits_missing`). Staff enter the schedule (see [Enter Income Limits](../income-limits/enter-income-limits.md)) and record again.
- **No income document chosen for Other:** the browser stops the form; a request without one is refused (`income_documents_required`).
- **Transfer already Completed or Closed:** the button is not shown; a request is refused.

Continues in: [Complete Heir Transfer](complete-heir-transfer.md)
