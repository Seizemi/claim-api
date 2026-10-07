# syntax=docker/dockerfile:1

# One image for the whole application: the API (BFF) serving the React build from wwwroot.
# Build context: the folder holding both repositories side by side (ClaimApi/ and claim-client/).
# Run it with docker compose from ClaimApi (see README), or from that parent folder:
#   docker build -f ClaimApi/ModularMonolith/Dockerfile -t claimapi .
# What the build can see is limited by Dockerfile.dockerignore, next to this file.

# 1. Build the React app
FROM node:22-alpine AS frontend
WORKDIR /src
COPY claim-client/package.json claim-client/package-lock.json ./
# react-scripts 5 declares TypeScript <= 4 as an optional peer while the app uses TypeScript 5; the lockfile
# pins the working versions, so skip that peer check (npm 10, bundled with Node 22, rejects it).
RUN npm ci --legacy-peer-deps
COPY claim-client/ ./
# Everything in the React build is public. Source maps would publish the source code too.
ENV GENERATE_SOURCEMAP=false
RUN npm run build

# 2. Build and publish the .NET API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend
# Release by default. Visual Studio passes Debug when it starts the compose project with the debugger.
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ClaimApi/ ./
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish ModularMonolith/ModularMonolith.csproj -c $BUILD_CONFIGURATION -o /app /p:UseAppHost=false

# 3. Runtime image: published output only (no source code, no SDK, no secrets)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Mount point of the Data Protection keys volume (DataProtection:KeysDirectory), writable by the app user.
RUN mkdir /keys && chown app /keys
USER app
EXPOSE 8080

FROM base AS final
COPY --from=backend /app ./
COPY --from=frontend /src/build ./wwwroot
ENTRYPOINT ["dotnet", "ModularMonolith.dll"]
