# syntax=docker/dockerfile:1

# =========================
# Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["Compartilhei.slnx", "./"]

COPY ["src/Compartilhei.Api/Compartilhei.Api.csproj", "src/Compartilhei.Api/"]
COPY ["src/Compartilhei.Application/Compartilhei.Application.csproj", "src/Compartilhei.Application/"]
COPY ["src/Compartilhei.Domain/Compartilhei.Domain.csproj", "src/Compartilhei.Domain/"]
COPY ["src/Compartilhei.Infrastructure/Compartilhei.Infrastructure.csproj", "src/Compartilhei.Infrastructure/"]

# Restore
RUN --mount=type=secret,id=sixlabors_license \
    export SIXLABORS_LICENSE_KEY="$(cat /run/secrets/sixlabors_license)" && \
    dotnet restore \
        "src/Compartilhei.Api/Compartilhei.Api.csproj" \
        -p:SixLaborsLicenseKey="$SIXLABORS_LICENSE_KEY"

COPY . .

# Build
RUN --mount=type=secret,id=sixlabors_license \
    export SIXLABORS_LICENSE_KEY="$(cat /run/secrets/sixlabors_license)" && \
    dotnet build \
        "src/Compartilhei.Api/Compartilhei.Api.csproj" \
        --configuration Release \
        --no-restore \
        --output /app/build \
        -p:SixLaborsLicenseKey="$SIXLABORS_LICENSE_KEY"


# =========================
# Publish
# =========================
FROM build AS publish

RUN --mount=type=secret,id=sixlabors_license \
    export SIXLABORS_LICENSE_KEY="$(cat /run/secrets/sixlabors_license)" && \
    dotnet publish \
        "src/Compartilhei.Api/Compartilhei.Api.csproj" \
        --configuration Release \
        --no-restore \
        --output /app/publish \
        /p:UseAppHost=false \
        -p:SixLaborsLicenseKey="$SIXLABORS_LICENSE_KEY"


# =========================
# Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Compartilhei.Api.dll"]