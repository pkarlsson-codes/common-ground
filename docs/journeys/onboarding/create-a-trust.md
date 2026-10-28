# Create a Trust

1. User signs in (or registers a new account on the sign-in page) but does not belong to any trust yet
2. User is redirected to Onboarding ("You're not part of a trust yet")
3. Page explains: access requires either an invite from a trust admin (opening the invite link from email while signed in adds the user automatically) or creating a new trust
4. User clicks "Create a trust"
5. User is taken to the Create Trust page
6. User enters Trust Name
7. User clicks "Create trust and continue"
8. If Trust Name is blank, error shown: "Please enter a community land trust name."
9. If the user already belongs to a trust (409 response), error shown: "You already belong to a trust."
10. On any other failure, a generic error message is shown ("Could not create trust. Please try again." or the server-provided message)
11. On success, user's session is refreshed and they are taken into the app (redirected to Land Parcels)
12. A user who already belongs to a trust and opens Onboarding or Create Trust is redirected into the app
