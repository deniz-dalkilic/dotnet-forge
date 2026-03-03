# Authentication and Authorization

## External Id Token Exchange

This template is intentionally OSS-first and does not bundle a specific identity provider.

- User authentication is delegated to an external OpenID Connect/OAuth 2.0 provider (for example Entra ID, Auth0, Keycloak, Okta, or a self-hosted IdP).
- The API validates incoming JWT bearer tokens against configured authority/audience values.
- Service-to-service authentication should use an **External Id Token Exchange** approach:
  - receive an external subject token from the caller context,
  - exchange it with your selected IdP/token service for an internal API token,
  - forward only the exchanged token to downstream services.
- Role/claim mapping remains configurable in application settings.

This keeps local infrastructure minimal while still supporting enterprise federation and delegated authorization scenarios.

## Social / External OIDC Login

When using Google, Microsoft, or Apple sign-in, the backend validates the provider `id_token` directly using OIDC discovery metadata and JWKS keys.

- The backend validates token signature, issuer, audience (configured client id), lifetime, and optional nonce.
- After successful validation, the system maps external identity claims (`sub`, `email`, `name`) and then issues an internal JWT for API authorization.
- `email` should only be treated as trusted for account-linking or privileged flows when `email_verified == true`.

This keeps trust decisions server-side and prevents clients from bypassing validation by sending unverified profile data.
