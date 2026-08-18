# Projectio

Projectio is an ASP.NET Core Web API (targeting .NET 10) that implements user authentication and authorization, external Google OpenID Connect sign-in, and EF Core persistence.

What is in the repository:
- Program.cs: configures services (EF Core, Identity, JWT validation, Google OIDC, CORS, Swagger) and runs migrations on startup.
- Persistence: ApplicationDbContext and EF Core migrations for SQL Server.
- Security: JWT signing/validation, Identity user and role management, TokenGenerator helper.
- Controllers: AuthController (username/password login, token cookie), OIDCController (Google authorize/callback), AdminController (role updates).
- Migrations/DatabaseSeeder: seeds roles and an initial admin user in development.

Authentication flows:
- Local login issues a JWT and sets an HttpOnly cookie; the API validates the JWT by issuer/audience/signature.
- Google OIDC flow redirects users to Google, handles the callback, creates or finds the user, and issues the same JWT-based session.

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

## Secure SPA integration (recommended pattern)

For a separate React frontend, the project supports a hybrid flow that avoids exposing JWTs in URLs or local storage:

1. Backend completes external auth and generates a short-lived, single-use code.
2. Browser is redirected to the SPA with that code in the query string.
3. SPA redeems the code via POST `/api/auth/exchange`.
4. Server sets an HttpOnly, Secure cookie containing the JWT; the token is never exposed to JavaScript.
5. SPA calls `/api/auth/me` with `credentials: 'include'` to obtain the user profile.

This approach mitigates token leakage and reduces XSS risk. When using cookies, ensure CSRF protections are in place and CORS is restricted to your frontend origin.

---

## API (overview)
- POST `/api/auth/authenticate` — username/password login (development-friendly behavior)
- POST `/api/auth/exchange` — redeem one-time code; sets authentication cookie
- GET `/api/auth/me` — returns authenticated user profile
- GET `/api/OIDC/google/authorize` — start Google OAuth flow
- GET `/api/OIDC/google/complete` — OAuth callback
- POST `/api/admin/UpdateRoles/{userid}` — Admin-only role updates

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

## Roadmap (selected improvements)
- Refresh-token rotation and revocation
- Global exception handling and structured logging (Serilog)
- Unit and integration tests with CI (GitHub Actions)
- Dockerfile and simple deployment manifest

---

## Contributing & license
- License: MIT (add a LICENSE file when publishing)
- Contributions: open an issue or PR; include tests for new behavior

---

## Author
Dimitrios Argyropoulos — https://github.com/Washabitsu

If you'd like a demo or a short walkthrough tailored for interviews, open an issue or contact the author.
