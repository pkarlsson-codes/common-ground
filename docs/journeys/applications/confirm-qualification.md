# Confirm Qualification

When a household on the waiting list reports a new income or household size, staff [update the household](../households/edit-household.md), and its approved application takes the new figures. The application is then marked "Qualification to confirm". It stays Approved, keeps its approved date and place in line, and stays on the waiting list. But the household can't be chosen for a home until staff confirm it still qualifies. Staff check the new figures outside the app, as they do at approval.

1. User opens the application in one of these ways:
   - clicks a "Qualification to confirm" row on [Today](../today/work-through-today.md), or its "Confirm still qualifies" button, which opens the panel over Today
   - goes to Applications and clicks the application
   - goes to the [Waiting List](../waiting-list/view-waiting-list.md), where the row shows "Qualification to confirm", and clicks it
2. The application's detail panel shows a warning at the top: "Qualification to confirm since {date}: Income $65,000 → $72,000, Household size 3 → 4. This household can't be chosen for a home until staff confirm it still qualifies." It lists only the figures that changed
3. In Workflow Actions, user clicks "Confirm Still Qualifies"
4. "Confirm Qualification" panel opens. It shows what changed and since when, and has Confirmed Date set to today. The date can't be in the future or before the change
5. User clicks "Confirm Still Qualifies"
6. Toast: "Qualification confirmed"
7. Panel closes. The warning is gone, Qualification Confirmed shows the date and who confirmed it, and the Today item goes. The household can be chosen for a home again

If the household no longer qualifies, staff [remove it from the waiting list](remove-from-waiting-list.md) with the reason "No longer eligible" instead.

How the mark behaves:

- Only income and household size mark the application. A change to the accessibility need alone updates the application with no mark.
- Further changes while a mark is open don't add a second mark. The mark keeps the date of the first change, and staff confirm once against the latest figures. "What changed" always compares with the figures staff last checked: at approval or at the last confirmation.
- If the figures go back to exactly the ones staff last checked, for example when staff correct a mistyped income, the mark clears by itself. **Unconfirmed with a practitioner:** whether a trust would still want to confirm in that case.
- Any change marks the application, a drop in income too, because qualifying also checks the income is enough for a mortgage.
- A submitted application takes the new figures with no mark, because it hasn't been approved yet. A selected application keeps the figures it was chosen on; the trust checks eligibility again before closing. If the purchase falls through, the application returns to the waiting list and takes the household's current figures. If income or size changed during the purchase, it is marked.
- The application takes the change a moment after the household is saved, so the warning can take a second to appear.

Unconfirmed with a practitioner: whether a trust re-checks qualification when a waiting household reports a change, or only updates its file, and whether a marked household should be blocked from being chosen or only flagged. This journey blocks it. See issue #23.
