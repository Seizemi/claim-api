# ClaimApi

Backend API for a claim management application. Allows internal staff to submit, track, and process warranty claims.

## Getting Started

### Prerequisites

- [Docker](https://www.docker.com/get-started) and Docker Compose

### Run

```bash
docker compose up
```

The API will be available at `http://localhost:8080` and Swagger UI at `http://localhost:8080/swagger`.

### Apply database migrations

```bash
dotnet ef database update --project Claims/Modules.Claims.Infrastructure/Modules.Claims.Infrastructure.csproj --startup-project ModularMonolith/ClaimApi.csproj
```

### Local database access

`docker compose up` also starts a Postgres container. A `docker-compose.override.yml` (gitignored, local-only) exposes it to your host so you can connect with a desktop client (DBeaver, Azure Data Studio, pgAdmin, etc.) — you don't need to install a Postgres server locally, just a client:

- Host: `localhost`
- Port: `5432`
- Username: `postgres`
- Password: `postgres`
- Database: `claimapi`

If `docker-compose.override.yml` is missing (e.g. fresh clone), recreate it at the repo root:

```yaml
services:
  postgres:
    ports:
      - "5432:5432"
```
