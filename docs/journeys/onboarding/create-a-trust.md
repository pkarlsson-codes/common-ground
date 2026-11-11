# Create a Trust

1. User signs in (or registers a new account on the sign-in page) but does not belong to any trust yet
2. User is redirected to Onboarding ("You're not part of a trust yet")
3. Page explains: to work in an existing trust, ask one of its members for an invite link and open it (see [Join a Trust](join-a-trust.md)); otherwise create a new trust
4. User clicks "Create a trust"
5. User is taken to the Create Trust page
6. User enters Trust Name and picks a Time Zone (required; prefilled from the browser's time zone). The trust's time zone decides what "today" is and how dates derived from timestamps are shown
7. User clicks "Create trust and continue"
8. If Trust Name is blank, error shown: "Please enter a community land trust name."
9. If the user already belongs to a trust (409 response), error shown: "You already belong to a trust."
10. On any other failure, a generic error message is shown ("Could not create trust. Please try again." or the server-provided message)
11. On success, user's session is refreshed and they are taken into the app (landing on [Today](../today/work-through-today.md))
12. A user who already belongs to a trust and opens Onboarding or Create Trust is redirected into the app
