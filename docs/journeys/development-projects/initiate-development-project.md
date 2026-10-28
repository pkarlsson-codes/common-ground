# Initiate Development Project

1. User navigates to Development Projects
2. User clicks "Initiate Project"
3. "Initiate Development Project" panel opens with Project Type defaulted to "New Construction" and an empty Land Parcel field
4. User selects Project Type: "New Construction" or "Rehabilitation"
5. User searches for and selects a land parcel in the Land Parcel combobox
6. User enters a project Name
7. If Project Type is "Rehabilitation", a Homes field appears; user selects one or more homes registered on the chosen parcel, each labeled by home type and size (e.g. "Single Family — 3bd / 2ba"; the list loads once a parcel is selected; at least one home must be selected to submit)
8. User enters a Description
9. User clicks "Initiate Project" (disabled until Land Parcel, Name, and Description are set, and — for Rehabilitation — at least one Home is selected)
10. Toast: "Development project initiated"
11. Panel closes, new project appears at the top of the Development Projects table

If the API rejects the project (e.g. a blank name or description), the panel stays open and shows the reason.

Note: switching Project Type clears any selected Homes (unless the Homes field is locked by the entry point, e.g. when opened from a home's detail panel).

Note: this panel can also be opened from a Land Parcel's detail panel (see [initiate-development-project-from-parcel](../land-parcels/initiate-development-project-from-parcel.md)) or from a Home's detail panel (see [initiate-development-project-from-home](../homes/initiate-development-project-from-home.md)). In both cases the Land Parcel field is pre-filled and locked, steps 1-3 are skipped, and the panel opens directly with the entry point's pre-fill/lock behavior described in those docs; all other steps are the same.
