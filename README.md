# Projectio

Projectio is an ASP.NET Core Web API (targeting .NET 10) that implements user authentication and authorization, external Google OpenID Connect sign-in, and EF Core persistence.

What is in the repository:
- Program.cs: configures services (EF Core, Identity, JWT validation, Google OIDC, CORS, Swagger) and runs migrations on startup.
- Persistence: ApplicationDbContext and EF Core migrations for SQL Server.
- Security: JWT signing/validation, Identity user and role management, TokenGenerator helper.
- Controllers: AuthController (code exchange, refresh, revoke), OIDCController (authorize + Google callback), WellKnownController (public signing keys), AccountController (profile), AdminController (role updates).
- Migrations/DatabaseSeeder: seeds roles and an initial admin user in development.

Authentication flows:
- Google OIDC flow redirects users to Google, handles the callback, creates or finds the user, and hands a one-time code to the registered client (BFF), which exchanges it for a JWT and a refresh token.

Notes for reviewers:
- Secrets and connection strings are kept out of source (use User Secrets or environment variables).
- The project supports both header-based bearer tokens and cookie transport for the same JWT; CORS and CSRF considerations must be set appropriately when integrating a separate frontend.

---

## Highlights
- JWT authentication with ASP.NET Core Identity
- Google OpenID Connect (external login) and account provisioning
- Role-based authorization (Admin / Tester / User)
- EF Core + SQL Server with migrations
- Swagger for API exploration

---

## Tech stack
- .NET 10 (ASP.NET Core Web API)
- Entity Framework Core 10 + SQL Server
- ASP.NET Core Identity
- JWT (Microsoft.IdentityModel.Tokens)
- Google OpenID Connect
- AutoMapper, Swashbuckle (Swagger)

---

## Quickstart (local development)

1. Clone the repository

```bash
git clone https://github.com/Washabitsu/Projectio.git
cd Projectio
```

2. Provide configuration (User Secrets or environment variables)

Required configuration keys (examples):

- ConnectionStrings:DefaultConnection
- IsDevelopment (true/false)
- Jwt:Issuer, Jwt:Audience, Jwt:SigningKey (check Program.cs for exact names)
- Google:ClientId, Google:ClientSecret, Google:RedirectUris
- Frontend:Url (e.g., http://localhost:3000)

Use `dotnet user-secrets` locally or environment variables for CI. Do not commit secrets.

3. Apply migrations (optional — the app also migrates on startup)

```bash
dotnet ef database update
```

4. Run the API

```bash
dotnet run --project Projectio
```

Open the Swagger UI in development at `https://localhost:7256/swagger`.

---

## Login flow with a BFF (Authorization Code + PKCE)

Projectio is the authorization server for a Backend-for-Frontend (Portio). The browser only ever holds an opaque session cookie from the BFF; tokens never reach JavaScript or a URL.

Projectio does not need a public address. Portio passes the two browser-facing endpoints (`/api/OIDC/...` and the Google callback path) through to Projectio with the original `Host` header, so the Google redirect URI is registered on Portio's address: `https://<portio>/api/auth/google/callback`.

1. The BFF redirects the browser to `GET /api/OIDC/authorize` with `client_id`, `redirect_uri`, `state` and a PKCE `code_challenge` (S256).
2. Projectio checks the client and its registered redirect URI, then sends the user to Google.
3. Google returns to `GET /api/OIDC/google/complete`. Projectio finds or creates the user and redirects to the BFF with a one-time `code` (valid for 1 minute, single use) and the BFF's `state`.
4. The BFF calls `POST /api/auth/exchange` server to server with the code, its PKCE `code_verifier` and its client secret, and receives an access token (JWT) and a refresh token.
5. The BFF refreshes with `POST /api/auth/refresh` (refresh tokens are rotated: each one works once) and revokes with `POST /api/auth/revoke` on logout.

Clients are registered in configuration:

```
Clients__0__ClientId=portio-bff
Clients__0__ClientSecret=<secret>
Clients__0__RedirectUri=https://localhost:7287/bff/callback
```

---

## API (overview)
- GET `/api/OIDC/authorize` - start a login for a registered client (PKCE required)
- GET `/api/OIDC/google/complete` - Google callback; redirects to the client with a one-time code
- POST `/api/auth/exchange` - client swaps code + code_verifier for tokens
- POST `/api/auth/refresh` - client swaps a refresh token for new tokens (rotation)
- POST `/api/auth/revoke` - client revokes a refresh token (logout)
- GET `/.well-known/jwks.json` - public key(s) that sign the access tokens, for services that validate them (the token header carries the matching `kid`)
- GET `/api/account` - profile of the user in the access token
- POST `/api/admin/UpdateRoles/{userid}` - Admin-only role updates

Use Swagger for full request/response definitions and sample payloads.

---

## Security posture (summary)
- Token validation enforces issuer, audience, signing key, and expiration
- Identity lockout configured to reduce brute-force attempts
- Token transport options support secure cookie use or header-based bearer tokens for different clients

Before production deployment:
- Restrict CORS to the exact frontend origin(s)
- Move secrets to a secure store and never commit them to source control
- Add structured logging and global exception handling

---

## Docker

```bash
docker build -t projectio .
```

The image listens on plain HTTP port 8080 and runs as a non-root user. It is meant to sit behind a proxy that ends HTTPS: set `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` so the app trusts the proxy's `X-Forwarded-Proto` header. All settings come from environment variables (see `Projectio/.env.example`).

---

## Roadmap (selected improvements)
- Global exception handling and structured logging (Serilog)
- Unit and integration tests with CI (GitHub Actions)

---

## Contributing & license
- License: MIT (add a LICENSE file when publishing)
- Contributions: open an issue or PR; include tests for new behavior

---

## Author
Dimitrios Argyropoulos — https://github.com/Washabitsu

If you'd like a demo or a short walkthrough tailored for interviews, open an issue or contact the author.
