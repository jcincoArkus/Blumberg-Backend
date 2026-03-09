ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Copy project files first so restore can be cached until dependencies change.
COPY Apps/API/API.csproj Apps/API/
COPY Apps/CLI/CLI.csproj Apps/CLI/
COPY Modules/Modules.csproj Modules/
COPY Modules/Auth/Modules.Auth.csproj Modules/Auth/
COPY Modules/Permissions/Modules.Permissions.csproj Modules/Permissions/
COPY Adapters/Server/Adapters.Server.csproj Adapters/Server/
COPY Adapters/Config/Adapters.Config.csproj Adapters/Config/
COPY Adapters/Database/Adapters.Database.csproj Adapters/Database/
COPY Adapters/Jwt/Adapters.Jwt.csproj Adapters/Jwt/
COPY Adapters/Logger/Adapters.Logger.csproj Adapters/Logger/
COPY Adapters/OpenApi/Adapters.OpenApi.csproj Adapters/OpenApi/
COPY Adapters/Permissions/Adapters.Permissions.csproj Adapters/Permissions/
COPY Adapters/Telemetry/Adapters.Telemetry.csproj Adapters/Telemetry/
COPY Adapters/Email/Adapters.Email.csproj Adapters/Email/
COPY Shared/Shared.csproj Shared/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "Apps/API/API.csproj"

# Copy full source only after restore for better cache efficiency.
COPY . .

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "Apps/API/API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    -p:UseAppHost=false

# Publish CLI for running nukeAndPave and ingestion:simulate inside the container (ECS Exec).
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "Apps/CLI/CLI.csproj" \
    -c Release \
    -o /app/cli-publish

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

LABEL org.opencontainers.image.title="blumberg-api" \
      org.opencontainers.image.description="Blumberg backend API service"

ENV ASPNETCORE_URLS=http://0.0.0.0:5000 \
    ASPNETCORE_HTTP_PORTS=5000 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

EXPOSE 5000

COPY --from=build /app/publish ./
COPY --from=build /app/cli-publish ./cli/

# Use the non-root user provided by official .NET images.
USER $APP_UID

ENTRYPOINT ["dotnet", "API.dll"]
