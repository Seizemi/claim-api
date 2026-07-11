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
dotnet ef database update --project src/Modules.[Name].Infrastructure
```
