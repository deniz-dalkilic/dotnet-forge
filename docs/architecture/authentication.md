# Authentication and Authorization

## External Id Token Exchange

This template is intentionally OSS-first and does not bundle a specific identity provider.

- User authentication is delegated to an external OpenID Connect/OAuth 2.0 provider (for example Entra ID, Auth0, Keycloak, Okta, or a self-hosted IdP).
- The API validates incoming JWT bearer tokens against configured authority/audience values.

For service-to-service scenarios, use an **External Id Token Exchange** pattern:

1. The caller obtains a token from its external identity provider in the current user/app context.
2. The caller sends that token as a **subject token** to your token service or federation boundary.
3. The token service validates the external token and exchanges it for an internal API token scoped to your backend.
4. Downstream services receive only the internal exchanged token (not the original external token).

Benefits of this architecture:

- Keeps provider-specific trust logic concentrated in one place.
- Reduces token sprawl across internal services.
- Allows consistent internal claims/role mapping regardless of external provider differences.

Role/claim mapping remains configurable in application settings.

## Social / External OIDC Login

When using Google, Microsoft, or Apple sign-in, the backend validates the provider `id_token` directly using OIDC discovery metadata and JWKS keys.

- The backend validates token signature, issuer, audience (configured client id), lifetime, and optional nonce.
- After successful validation, the system maps external identity claims (`sub`, `email`, `name`) and then issues an internal JWT for API authorization.
- `email` should only be treated as trusted for account-linking or privileged flows when `email_verified == true`.

This keeps trust decisions server-side and prevents clients from bypassing validation by sending unverified profile data.

## Provider setup checklists

### Google

- [ ] Create an OAuth client in Google Cloud Console for your app type (Web/iOS/Android).
- [ ] Set the backend configuration `ExternalAuth__Providers__Google__ClientId` to that OAuth client id.
- [ ] Ensure incoming `id_token.aud` exactly matches your configured Google client id.

### Microsoft Entra ID

- [ ] Create an app registration in Microsoft Entra ID.
- [ ] Use the app (client) id as `ExternalAuth__Providers__Microsoft__ClientId`.
- [ ] Choose tenant behavior intentionally:
  - `common` for multi-tenant/work-school + personal account scenarios.
  - specific tenant id/domain when you want tenant-restricted sign-in.

### Apple

- [ ] Configure Apple Sign in and use the correct client id for your flow:
  - Service ID for web-based flows.
  - Bundle ID for native app flows where applicable.
- [ ] Set that value as `ExternalAuth__Providers__Apple__ClientId`.
- [ ] If your client sends a nonce, pass it through and validate it server-side against the `id_token` nonce claim.

## Security checklist

- [ ] Validate token **signature**, `iss`, `aud`, and `exp` on every external `id_token`.
- [ ] Nonce verification is strongly recommended when the client provides a nonce.
- [ ] Do not trust `email` unless provider verification indicates it is verified.
- [ ] Avoid automatic account linking by email unless that email is verified.

## Optional Refresh Tokens

Refresh token support is optional and controlled by configuration (`RefreshTokens:Enabled`).

- Refresh tokens are stored hashed (SHA-256), rotated on use, and can be revoked on logout.
- This is suitable for a monolith or small system where auth remains in-process.
- For microservices, consider a dedicated auth service/token service boundary instead of sharing refresh token concerns across services.
