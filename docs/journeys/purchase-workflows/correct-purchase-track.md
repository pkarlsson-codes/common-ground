# Correct a Purchase Track

Staff recorded an income, appraised value or financing amount that was wrong. A correction replaces the figures, keeps the old ones in history and needs a reason. It works on Eligibility, Appraisal and Financing, while the purchase is not Closed, Failed or Withdrawn.

1. User navigates to Purchases
2. User clicks a row for a non-terminal workflow to open the detail panel
3. In Purchase Tracks, user expands a track that is marked complete (Eligibility, Appraisal or Financing) and clicks "Correct"
4. The form opens with the recorded values filled in:
   - Eligibility: Verified Date, Verified Annual Income, Verified Household Size, Notes and the income documents. The recorded documents stay unless the user chooses new ones, which replace them
   - Appraisal: Appraised Value, Appraisal Date, Appraiser Name, Notes
   - Financing: Lender Name, Committed Amount, Pre-Approval Letter Date
5. User changes the figures and enters a Reason (required)
6. For Eligibility the panel shows the limit and band for the entered income and size, as in [Record Eligibility Verified](record-eligibility-verified.md)
7. User clicks "Save Correction"
8. Toast: "Correction saved"
9. The track shows the new values. A Corrections list under Purchase Tracks gains an entry, newest first: the track, the date, the user, the reason and "was → now" for each field that changed, for example "Appraised value: $2,400,000 → $240,000"

When the purchase closes, the new ground lease takes the corrected Appraised Value (see [Record Purchase Closed](record-purchase-closed.md)).

## Outcomes

- **No reason:** the browser stops the form; a request without one is refused (`reason_required`). Nothing changes.
- **Nothing changed:** refused (`no_change`). Nothing is added to history.
- **Eligibility income over the limit:** refused (`over_income_limit`), as when recording. The recorded figures stay.
- **Appraised value zero or below, household size below 1, income below zero:** refused, as when recording.
- **Purchase Closed, Failed or Withdrawn:** the Correct button is not shown; a request is refused. Correcting a closed purchase is not built.
- **Track not recorded yet:** the track shows its Record form, not Correct.
