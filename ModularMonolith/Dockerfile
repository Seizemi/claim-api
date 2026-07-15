# Consultez https://aka.ms/customizecontainer pour savoir comment personnaliser votre conteneur de débogage et comment Visual Studio utilise ce Dockerfile pour générer vos images afin d’accélérer le débogage.

# Cet index est utilisé lors de l’exécution à partir de VS en mode rapide (par défaut pour la configuration de débogage)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Cette phase est utilisée pour générer le projet de service
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ModularMonolith/ClaimApi.csproj", "ModularMonolith/"]
COPY ["Claims/Modules.Claims.Features/Modules.Claims.Features.csproj", "Claims/Modules.Claims.Features/"]
COPY ["Common/Modules.Common.Features/Modules.Common.Features.csproj", "Common/Modules.Common.Features/"]
COPY ["Claims/Modules.Claims.Infrastructure/Modules.Claims.Infrastructure.csproj", "Claims/Modules.Claims.Infrastructure/"]
COPY ["Claims/Modules.Claims.Domain/Modules.Claims.Domain.csproj", "Claims/Modules.Claims.Domain/"]
RUN dotnet restore "./ModularMonolith/ClaimApi.csproj"
COPY . .
WORKDIR "/src/ModularMonolith"
RUN dotnet build "./ClaimApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Cette étape permet de publier le projet de service à copier dans la phase finale
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ClaimApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Cette phase est utilisée en production ou lors de l’exécution à partir de VS en mode normal (par défaut quand la configuration de débogage n’est pas utilisée)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClaimApi.dll"]