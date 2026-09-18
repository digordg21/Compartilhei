# =========================
# Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copia os projetos
COPY ["Compartilhei.sln", "./"]

COPY ["src/Compartilhei.Api/Compartilhei.Api.csproj", "src/Compartilhei.Api/"]
COPY ["src/Compartilhei.Application/Compartilhei.Application.csproj", "src/Compartilhei.Application/"]
COPY ["src/Compartilhei.Domain/Compartilhei.Domain.csproj", "src/Compartilhei.Domain/"]
COPY ["src/Compartilhei.Infrastructure/Compartilhei.Infrastructure.csproj", "src/Compartilhei.Infrastructure/"]

# Restore
RUN dotnet restore "src/Compartilhei.Api/Compartilhei.Api.csproj"

# Copia o restante do código
COPY . .

# Build
RUN dotnet build "src/Compartilhei.Api/Compartilhei.Api.csproj" \
    -c Release \
    --no-restore \
    -o /app/build


# =========================
# Publish
# =========================
FROM build AS publish

RUN dotnet publish "src/Compartilhei.Api/Compartilhei.Api.csproj" \
    -c Release \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false


# =========================
# Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=publish /app/publish .

# Azure Container Apps / .NET 8+
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "Compartilhei.Api.dll"]
