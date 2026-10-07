# ClaimApi

Backend API for a claim management application. Allows internal staff to submit, track, and process warranty claims.

## Run locally

The application (API + React app) can be started in two ways. Both open at **`https://localhost:7001`**, use the **dev tenant** for sign-in (same redirect URI), and need HTTPS: the `__Host-` session cookie is only accepted over a secure connection.

| | A. Day-to-day development | B. Docker (real image) |
|---|---|---|
| What runs | `dotnet run` (Debug) + React dev server | The container image built by `ModularMonolith/Dockerfile` |
| React app | Served by `npm start`, through the API (YARP proxy) | Built into the image, served from `wwwroot` |
| Hot reload | Yes (React), C# with restart or Hot Reload | No: rebuild the image |
| Environment | `Development` | `Production`, as in Azure |
| Settings | `dotnet user-secrets` | `.env.local` |
| Use it to | Write and debug code | Check the Dockerfile and the app as deployed |

### Prerequisites

- [Docker Desktop](https://www.docker.com/get-started) (Postgres always runs in Docker)
- The .NET 10 SDK and Node.js 22
- `ClaimApi` and `claim-client` cloned **side by side** in the same folder (the Docker image is built from both):
  ```
  <folder>/
    ClaimApi/
    claim-client/
  ```
- Dev tenant values (authority, tenant ID, client ID, client secret): see [Values to collect per tenant](#values-to-collect-per-tenant). Never commit them.
- Once, in `claim-client`: `npm install`

The database is created, migrated and seeded (1000 claims) automatically when the API starts.

### A. Day-to-day development (`dotnet run` + React dev server)

The browser always uses the API's origin. In Development, the API forwards every request except `/api/*`, `/auth/*`, `/signin-oidc` and `/signout-callback-oidc` to the React dev server on port 3000, so cookies and sign-in behave exactly as in production.

**Once:** store the dev tenant values in user-secrets ([checklist step 8](#checklist-phase-1-sign-in)):
```powershell
cd ModularMonolith
dotnet user-secrets set "AzureAd:Authority" "https://<subdomain>.ciamlogin.com/"
dotnet user-secrets set "AzureAd:TenantId" "<tenant-id>"
dotnet user-secrets set "AzureAd:ClientId" "<web-client-id>"
dotnet user-secrets set "AzureAd:ClientSecret" "<web-client-secret>"
```

**Each time**, from the command line:
1. Postgres, from `ClaimApi`: `docker compose up -d postgres`
2. The API, from `ClaimApi`: `dotnet run --project ModularMonolith --launch-profile https`
3. The React dev server, from `claim-client`: `npm start` (no browser opens: port 3000 is not meant to be used directly)
4. Open `https://localhost:7001`.

**Or from Visual Studio:** start Postgres (step 1), set `ModularMonolith` as the startup project, choose the **`https`** launch profile and press F5. Then run `npm start` in `claim-client`.

### B. Docker (`docker compose up`)

Runs the real container image: the API serving the React build, in `Production`, as in Azure.

**Once:**
1. Export the ASP.NET Core development certificate with a password of your choice:
   ```powershell
   dotnet dev-certs https -ep "$env:USERPROFILE\.aspnet\https\claimapi.pfx" -p '<password>'
   dotnet dev-certs https --trust
   ```
   Keep the single quotes: PowerShell otherwise alters passwords containing characters such as `;` `,` `@` `(` `` ` `` `&` or `$`. "A valid HTTPS certificate is already present" is expected: the existing development certificate is exported.
2. In `ClaimApi`, copy `.env.local.example` to **`.env.local`** (git-ignored) and fill it in:
   - the dev tenant values: the user-secrets keys with `__` instead of `:` (for example `AzureAd__ClientId`). `AzureAd__Authority` is the full URL, `https://<subdomain>.ciamlogin.com/`, with `https://` only once;
   - `ASPNETCORE_Kestrel__Certificates__Default__Password`: the certificate password, without quotes. Avoid `$`, which Docker Compose treats as a variable.

**Each time**, from the command line in `ClaimApi`:
1. `docker compose up --build` (`--build` makes sure the image contains your latest code)
2. Open `https://localhost:7001`.
3. Stop with Ctrl+C, or `docker compose down`.

**Or from Visual Studio:** set **`docker-compose`** as the startup project and press F5. Visual Studio builds the full image (in Debug, so breakpoints work) and attaches the debugger; Ctrl+F5 starts it without debugging. Don't use the `Container (Dockerfile)` profile of `ModularMonolith`: it starts the API alone, without Postgres or `.env.local`.

About the image: it contains the compiled API and the React build only (no source code, no source maps, no secrets) and runs as the non-root `app` user. `ModularMonolith/Dockerfile.dockerignore` keeps tests, build outputs, `node_modules` and every `.env*` file out of the build. The keys that encrypt the session cookie are kept in the `dataprotection-keys` volume, so restarting the container doesn't sign users out.

### Good to know

- **Use only one way at a time:** both listen on port 7001.
- **One session per browser:** all tabs share the session cookie. To test two users at once (for example a Supervisor and an Agent), use a normal and a private window, or two browsers.
- **The session ends** `Session:LifetimeHours` (8) after sign-in, even for an active user. Signing out returns to the dashboard.
- Every endpoint requires a session. State-changing `/api/*` requests also need the `X-CSRF: 1` header (the React app adds it). `/healthz` is the anonymous liveness check, `GET /api/me` returns the signed-in user and roles.

### Troubleshooting

| Symptom | Cause and fix |
|---|---|
| The API stops at startup naming an `AzureAd` key | The value is missing or still a placeholder: set it in user-secrets (A) or `.env.local` (B). |
| `The certificate data cannot be read with the provided password` (B) | The password in `.env.local` doesn't match the exported certificate. Re-export with the password in single quotes, and make sure `.env.local` is saved. |
| `Unable to retrieve document from 'https://https://...'` or sign-in fails right away | `AzureAd:Authority` is wrong (for example `https://` written twice). |
| Port 7001 already in use | The other way is still running: stop `dotnet run` or `docker compose down`. |
| Pages don't load with way A | The React dev server isn't running: `npm start` in `claim-client`. |
| Old code in the container (B) | Start with `docker compose up --build`. |
| Everyone is signed out after `docker compose down -v` | Expected: `-v` deletes the volumes, including the session keys (and the database). |
| `libgssapi_krb5.so.2` in the container logs | Harmless: the Postgres driver looks for Kerberos, which isn't used. |

### Apply database migrations manually

Migrations run automatically at startup. To apply them by hand:

```bash
dotnet ef database update --project Claims/Modules.Claims.Infrastructure/Modules.Claims.Infrastructure.csproj --startup-project ModularMonolith/ModularMonolith.csproj
```

### Local database access

Postgres runs in Docker in both ways and is exposed to your host, so you can connect with a desktop client (DBeaver, Azure Data Studio, pgAdmin, etc.) without installing a Postgres server:

- Host: `localhost`
- Port: `5432`
- Username: `postgres`
- Password: `postgres`
- Database: `claimapi`


## Authentication: Entra External ID tenant setup

The application signs users in with **Microsoft Entra External ID** using the Backend-for-Frontend (BFF) pattern: the API is a confidential client, holds the session in an encrypted cookie, and no token ever reaches the browser. The full design is in [`Document/Authentication/partner-user-management-feature.md`](../Document/Authentication/partner-user-management-feature.md).

There are **two external tenants**, one for **dev** and one for **prod**. They are configured **identically** with this checklist: every change made in one is reproduced in the other. Only the tenant-specific IDs differ. The role GUIDs are the same in both.

### Fixed values (identical in every tenant)

| Item | Value |
|---|---|
| Web app registration name | `partner-portal-web` |
| App role `Supervisor`: value / ID | `Supervisor` / `c8e346c2-7745-4b10-9754-fd9e7566363d` |
| App role `Agent`: value / ID | `Agent` / `7a412abe-ac73-4ed9-ad1e-5c3c8fceef53` |
| Sign-in callback path | `/signin-oidc` |
| Sign-out callback path | `/signout-callback-oidc` |
| Local dev URL | `https://localhost:7001` |

### Checklist: phase 1 (sign-in)

**1. Create the external tenant**
- [ ] [Microsoft Entra admin center](https://entra.microsoft.com), *Entra ID > Overview > Manage tenants > Create > External*. Name: `claim-dev` (or `claim-prod`).
- [ ] Link the tenant to an Azure subscription (needed for billing and some features).
- [ ] Record the **tenant ID** and **domain** (`<subdomain>.onmicrosoft.com`). The authority is `https://<subdomain>.ciamlogin.com/`.
- [ ] From now on, switch the admin center to the external tenant (*Settings > Directories + subscriptions*).

**2. Register `partner-portal-web`**
- [ ] *App registrations > New registration*. Name `partner-portal-web`, supported account types: *Accounts in this organizational directory only*.
- [ ] Platform **Web** (not SPA). Redirect URI: `https://localhost:7001/signin-oidc` (dev tenant). Prod tenant: `https://<prod-host>/signin-oidc` plus the staging slot host.
- [ ] *Authentication*: front-channel logout URL `https://localhost:7001/signout-callback-oidc`. Leave *Access tokens* and *ID tokens* (implicit flow) **unchecked**: the BFF uses the authorization code flow.
- [ ] *Certificates & secrets*: dev tenant only, a **client secret** is allowed (store it in user-secrets, never commit it). Prod tenant: a **certificate** stored in Key Vault.
- [ ] *API permissions*: keep `openid`, `profile`, `offline_access` and `User.Read` (delegated). No exposed API scope is needed.
- [ ] *API permissions > Grant admin consent for <tenant>*. External users can't consent themselves: without this step, sign-in fails after the password page with a consent error.
- [ ] Record the **Application (client) ID**.

**3. App roles (fixed GUIDs)**

The portal generates a random ID for each role, so create the roles in the **Manifest** to keep the IDs above. In *App registrations > partner-portal-web > Manifest*, set `appRoles` to:

```json
"appRoles": [
  {
    "allowedMemberTypes": [ "User" ],
    "description": "Manages Agents and has every Agent right.",
    "displayName": "Supervisor",
    "id": "c8e346c2-7745-4b10-9754-fd9e7566363d",
    "isEnabled": true,
    "value": "Supervisor"
  },
  {
    "allowedMemberTypes": [ "User" ],
    "description": "Uses the business features of the application.",
    "displayName": "Agent",
    "id": "7a412abe-ac73-4ed9-ad1e-5c3c8fceef53",
    "isEnabled": true,
    "value": "Agent"
  }
]
```

- [ ] Save, then check under *App roles* that both roles exist with these IDs.

**4. Enterprise application**
- [ ] *Enterprise applications > partner-portal-web > Properties*: **Assignment required = Yes**. Users without a role can't sign in.
- [ ] Record the enterprise app's **Object ID** (the service principal object ID, needed in phase 2).

**5. User flow**
- [ ] *Entra ID > External Identities > User flows > New user flow*: name `signin`, *Identity providers* > **Email Accounts** > **Email with password**. *User attributes*: **Display Name**. *Create*.
- [ ] Open the flow, *Use > Applications > Add application*, select `partner-portal-web`. An application can have only one user flow.
- [ ] Turn off self-service sign-up (decision D1). There's no switch in the portal; it's a Microsoft Graph call ([docs](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-disable-sign-up-user-flow)), for example from Graph Explorer signed in to the external tenant with `EventListener.ReadWrite.All`:
  1. Find the flow's ID: `GET https://graph.microsoft.com/beta/identity/authenticationEventsFlows?$filter=microsoft.graph.externalUsersSelfServiceSignUpEventsFlow/conditions/applications/includeApplications/any(appId:appId/appId eq '<web-client-id>')`
  2. `PATCH https://graph.microsoft.com/beta/identity/authenticationEventsFlows/<user-flow-id>` with:
     ```json
     {
       "@odata.type": "#microsoft.graph.externalUsersSelfServiceSignUpEventsFlow",
       "onInteractiveAuthFlowStart": {
         "@odata.type": "#microsoft.graph.onInteractiveAuthFlowStartExternalUsersSelfServiceSignUp",
         "isSignUpAllowed": false
       }
     }
     ```

**6. Self-service password reset** ([docs](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-enable-password-reset-customers))
- [ ] *Entra ID > Authentication methods > Policies > Email OTP*: *Enable and Target* on, *Include* **All users**, *Save*.
- [ ] *Company Branding > Default sign-in > Edit > Sign-in form*: check **Show self-service password reset**, *Review + save*.

**7. Test users (dev tenant only)**
- [ ] *Users > New user > **Create new external user*** (not *Create new user*, which creates an internal account without an email identity). *Identities*: sign-in method **Email**, value = a real mailbox you can read. Create a test Supervisor, then a test Agent.
- [ ] *Enterprise applications > partner-portal-web > Users and groups > Add user/group*: assign `Supervisor` to the first user and `Agent` to the second.
- [ ] Optional: create a third user **without** an assignment to check that sign-in is refused.
- [ ] Sign in once as each user through the flow (*User flows > signin > Run user flow*, reply URL `https://localhost:7001/signin-oidc`) and set the password, either with the generated password (Entra then asks for a new one) or with *Forgot password?*. An error page at localhost is expected after sign-in until the backend is ready (step 1.2).

**8. Store the dev values locally** (read by the backend since step 1.2)

```bash
cd ModularMonolith
dotnet user-secrets set "AzureAd:Authority" "https://<subdomain>.ciamlogin.com/"
dotnet user-secrets set "AzureAd:TenantId" "<tenant-id>"
dotnet user-secrets set "AzureAd:ClientId" "<web-client-id>"
dotnet user-secrets set "AzureAd:ClientSecret" "<web-client-secret>"
```

### Values to collect per tenant

| Value | Where to find it |
|---|---|
| Tenant ID | Tenant *Overview* > Directory (tenant) ID |
| Tenant domain (`<subdomain>.onmicrosoft.com`) | Tenant *Overview* > Primary domain |
| Authority (`https://<subdomain>.ciamlogin.com/`) | Built from the tenant domain |
| `partner-portal-web` client ID | App registration *Overview* > Application (client) ID |
| `partner-portal-web` service principal object ID | *Enterprise applications* > partner-portal-web > Object ID (not the app registration's Object ID) |

Don't write the actual values in this README or in any committed file. Dev values go in `dotnet user-secrets` (and `.env.local` for Docker). Prod values go in the App Service settings. Secrets (client secret, certificates) go in user-secrets locally and in Key Vault in Azure.

### Creating a Supervisor

A Supervisor can't be created from the application. Create the user in the Entra admin center (*Users > New user > Create new external user*, sign-in method Email), then assign the `Supervisor` role in *Enterprise applications > partner-portal-web > Users and groups*. Do this in each tenant.
