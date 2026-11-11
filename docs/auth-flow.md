# Authentication Flow

How a user gets a session and how that session becomes an authenticated API call. Applies to the Management Gateway (:4000); the Homeowner Gateway follows the same pattern. Rendered viewer: [auth-flow.html](auth-flow.html).

Key facts:

- The gateway is the only OIDC client. The browser never talks to Keycloak directly except during the redirect dance, and Keycloak tokens never reach the browser or the API.
- Upstream tokens (access/refresh/id) live server-side in the gateway's `sessions` table in Postgres (7-day expiry, expired rows deleted hourly), keyed by a random session ID stored in the `cg_session` cookie (HttpOnly). `cg_csrf` is JS-readable for CSRF protection; `cg_auth` is the ASP.NET cookie-auth ticket.
- Every gateway→API call carries a freshly minted internal JWT (HS256, 5-minute lifetime, issuer `CommonGround.Gateway`, audience `CommonGround.API`). The API derives identity from its `user_id` claim; `tenant_id` carries the trust ID.

## Login

```mermaid
sequenceDiagram
    autonumber
    participant B as Browser
    participant G as Gateway
    participant K as Keycloak
    participant R as Session store (Postgres)
    participant A as API

    B->>G: GET /auth/login?returnUrl=…
    G-->>B: 302 to Keycloak authorize (PKCE + state)
    B->>K: authorize request, user signs in
    K-->>B: 302 to gateway OIDC callback (authorization code)
    B->>G: callback with code
    G->>K: exchange code for tokens
    K-->>G: access + refresh + id tokens
    Note over G: OnTokenValidated
    G->>A: EnsureUser(sub, email, name, trustId?)<br/>service JWT (sub=gateway-service, actor=gateway)
    A-->>G: canonical userId + trustId (created if new)
    G->>R: save tokens under random sessionId<br/>(userId, trustId, access/refresh/id)
    G-->>B: Set-Cookie cg_auth, cg_session (HttpOnly), cg_csrf<br/>302 to returnUrl
```

Note step 8: user provisioning is just-in-time — the gateway calls the API's `EnsureUser` during login using a *service* token (no `user_id` yet), and the API's canonical user ID is what goes into the session, not the Keycloak subject.

## Authenticated API call

```mermaid
sequenceDiagram
    autonumber
    participant B as Browser
    participant G as Gateway
    participant R as Session store (Postgres)
    participant A as API

    B->>G: GET /api/… (cookies: cg_auth, cg_session)
    Note over G: cookie auth establishes principal
    Note over G: InternalGatewayAuthHandler (on outbound API client)
    G->>G: user_id from claims principal
    G->>R: lookup session by cg_session
    R-->>G: userId (fallback), trustId
    G->>G: mint internal JWT (5 min, HS256)<br/>claims: user_id, tenant_id, session_id
    G->>A: forward request, Bearer internal JWT
    Note over A: validate issuer/audience/signature,<br/>identity = user_id claim
    A-->>G: response
    G-->>B: response
```

The trust ID always comes from the session lookup, not from claims — a request with a valid `cg_auth` cookie but no session row gets a JWT without `tenant_id`.

## Token refresh

```mermaid
sequenceDiagram
    autonumber
    participant B as Browser
    participant G as Gateway
    participant R as Session store (Postgres)
    participant K as Keycloak

    B->>G: POST /auth/refresh (cg_session)
    G->>R: lookup refresh token
    alt no session or no refresh token
        G-->>B: 401
    end
    G->>K: refresh_token grant
    K-->>G: new access (+ refresh) token
    G->>R: remove old sessionId,<br/>save tokens under NEW sessionId
    G-->>B: Set-Cookie new cg_session + cg_csrf, 200
```

Refresh rotates the session ID: the old session row is deleted and new cookies are issued, so a captured `cg_session` value goes stale after the next refresh.

## Logout

No diagram — `GET /auth/logout` deletes the session row and the `cg_session`/`cg_csrf` cookies, then signs out of both the cookie scheme and Keycloak (passing the stored `id_token` as the logout hint).
