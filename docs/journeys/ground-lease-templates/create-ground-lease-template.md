# Create Ground Lease Template

1. User navigates to Ground Lease Templates
2. User clicks "New Template"
3. "New Template" panel opens with a fresh form (Cancel discards anything entered)
4. User enters Template Name, Term (years, default 99), Option Window (days the trust has to buy the home after the homeowner gives notice to sell, default 60), the monthly Land Use Fee and Repair Reserve Fee ($, both required, zero or more, both default 0; the form shows their sum as the Monthly Lease Fee, due on the 1st), the late interest terms (Model Lease Art 5.7: Late Interest Rate, % a year, default 0, at most 100, 0 charges no interest; and Interest Waived If Paid Within, days, default 30, disabled while the rate is 0; the form shows the term as "Late Interest: 10% a year, waived if paid within 30 days" or "Late Interest: None"), and Covenant Terms. A lease from this template copies the fees and the interest terms when its purchase closes
5. User selects a Resale Formula: Appraisal Based, Index Based, or Fixed Percentage (default Appraisal Based)
6. If Appraisal Based, user enters Appreciation Share (decimal fraction, e.g. 0.25 = 25%, default 0.25)
7. If Index Based, user selects Index Type (CPI, AMI, or Custom, default CPI) and enters Baseline Index Value (default 100)
8. If Fixed Percentage, user enters Annual Percentage (decimal fraction, e.g. 0.03 = 3%, default 0.01)
9. User optionally checks "Set improvements cap" and enters an Improvements Cap ($, default 25000 when first checked)
10. User enters Debt Cap (% of the purchase option price, whole number, 1 to 200, required, no default; help text "The most total mortgage debt may be when the trust permits a new loan (Art 8.3). Commonly 80–100%."). A lease from this template copies it
11. User clicks "Create Template"
12. Toast: "Ground lease template created"
13. Panel closes, new template appears in the Ground Lease Templates table
