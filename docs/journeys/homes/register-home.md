# Register Home

1. User navigates to Homes
2. User clicks "Register Home"
3. "Register Home" panel opens with an empty Land Parcel field
4. User searches for and selects a land parcel in the Land Parcel combobox
5. User selects a home type (Single Family, Condo Unit, Duplex, or Townhouse)
6. User optionally enters a Unit Designation (e.g. "Unit B") for a parcel with several homes; the home is then labelled with the parcel address and the unit, e.g. "14 Alder Lane, Unit B"
7. User enters bedrooms, bathrooms, square footage, and year built, and ticks "Accessible home" if the home is accessible to people with disabilities (unticked by default)
8. User sets Income Limit as a Percent of AMI (defaults to 80), or ticks "No income limit" for a home sold at market rate
9. User enters an initial assessed value and assessment date, and optionally an appraiser name
10. User clicks "Register Home"
11. Toast: "Home registered successfully"
12. Panel closes, new home appears in the Homes table; the initial assessment appears in the home's Assessments list, and the home's detail shows Accessible: Yes or No and its income limit

Note: this panel can also be opened from a Land Parcel's detail panel (see [register-home-from-parcel](../land-parcels/register-home-from-parcel.md)). In that case the Land Parcel field is pre-filled and locked to the parcel the user came from, and steps 1-4 are skipped, all other steps are the same.
