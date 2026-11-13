# Record Mortgage Request

For when a homeowner asks to put a new loan on the home: a refinance, a second mortgage, a home repair loan (Art 8.3). The lease asks for the terms in writing at least 15 business days before the loan closes.

1. User opens a ground lease (see [View Ground Lease Detail](view-ground-lease-detail.md)) and clicks "Record Request" in the Mortgages section (Active leases only)
2. "Record Mortgage Request" panel opens; its subtitle is the homeowner's name. A line lists what the lease asks the homeowner to send: lender, reason, principal and total debt, closing costs, interest rate, repayment schedule, and the loan's appraisal
3. User enters:
   - Received On (default today, not in the future) and Expected Closing (required, not before Received On). Below them: "{n} business days from receipt to closing", in amber with "The lease asks for at least 15." when fewer than 15
   - Lender (required) and Purpose (Refinance, Second Mortgage, Home Repair or Other; default Refinance)
   - Principal ($, required, above zero) and Total Mortgage Debt After Closing ($, required, at least the principal; prefilled with the principal plus the other current permitted mortgages for any purpose but Refinance, and with the principal for Refinance; the help text says "All mortgage debt on the home once this loan closes, including loans it doesn't pay off" and, until the user changes the total, what it was filled in from)
   - Interest Rate (% a year, required), Rate Type (Fixed or Adjustable, default Fixed) and Term (years, required)
   - Closing Costs ($, optional)
   - Appraised Value ($, optional) and Appraisal Date (required when a value is entered, not in the future): the loan's appraisal. On an index-based lease, also Current Index Value (required when a value is entered)
4. User clicks "Record Request", or "Cancel"
5. Toast: "Request recorded". The Mortgage Request panel opens for it (see [View Mortgage Request](view-mortgage-request.md)), and the request appears under Requests in the Mortgages section and on Today
6. Refused while the lease has another request waiting for a decision: "This lease already has a mortgage request waiting for a decision."

Continues in: [View Mortgage Request](view-mortgage-request.md)
