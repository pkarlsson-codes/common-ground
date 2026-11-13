# Try the Demo

Only on a server with demo mode on. For CLT staff who want to look at CommonGround with sample records before deciding anything.

1. Signed-out visitor opens the site's home address and sees the Welcome page: what CommonGround is, what the sample trust holds, how many days it lives, and a warning not to enter real personal information
2. Visitor clicks "Try the demo", which opens the registration form, registers and confirms their email. "Sign in" opens the sign-in form for returning visitors
3. Visitor lands on Onboarding ("You're not part of a trust yet"), which offers "Try it with sample data" first and "Create an empty trust" second (see [Create a Trust](create-a-trust.md))
4. Visitor clicks "Try it with sample data"
5. The page shows "Filling your sample trust with a few years of records…" until it is ready, then takes the visitor to [Today](../today/work-through-today.md) in "Sample Land Trust"
6. If filling fails, the page says "Could not set up the sample trust." with a "Try again" button
7. If the demo already holds its maximum number of sample trusts, the page says "The demo is full right now. Try again tomorrow." and no sample trust is started
8. Every page in the trust shows a banner: "Sample data for trying CommonGround. Don't enter real personal information. This trust is deleted on {date}." with a "Send feedback" link that opens the operator's feedback address or form
9. A visitor who signs in again before the delete date returns to the same trust with their changes
10. Within an hour after the delete date the trust and everything in it is deleted: records, document files and scheduled reminders. On their next visit the visitor lands on Onboarding and can start a new sample trust

Notes:
- Only the home address shows the Welcome page. A signed-out visitor who opens any other link (an invite link, a bookmarked page) goes straight to the sign-in form, as on any server.
- In demo mode an empty trust from Create Trust is also deleted on its date, and shows the same banner.
- Members invited into a sample trust lose access when it is deleted.
- The visitor's sign-in account is kept, so they can sign in again and start a new sample trust. The operator removes accounts in the Keycloak admin console when asked.

Continues in: [Work through Today](../today/work-through-today.md)
