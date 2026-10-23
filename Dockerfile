# syntax=docker/dockerfile:1

# =========================================================
# ETAPA 1: BUILD — compila y publica la API
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restaurar dependencias en una capa propia (se cachea)
COPY Dsw2025Tpi.Api/Dsw2025Tpi.Api.csproj                 Dsw2025Tpi.Api/
COPY Dsw2025Tpi.Application/Dsw2025Tpi.Application.csproj Dsw2025Tpi.Application/
COPY Dsw2025Tpi.Data/Dsw2025Tpi.Data.csproj               Dsw2025Tpi.Data/
COPY Dsw2025Tpi.Domain/Dsw2025Tpi.Domain.csproj           Dsw2025Tpi.Domain/
COPY Dsw2025Tpi.Shared/Dsw2025Tpi.Shared.csproj           Dsw2025Tpi.Shared/
RUN dotnet restore Dsw2025Tpi.Api/Dsw2025Tpi.Api.csproj

# Copiar el resto del código y publicar en modo Release
COPY . .
RUN dotnet publish Dsw2025Tpi.Api/Dsw2025Tpi.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# =========================================================
# ETAPA 2: FINAL — imagen liviana solo para ejecutar
# =========================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
COPY --from=build /app/publish .

# Ejecutar sin privilegios de root
USER $APP_UID

ENTRYPOINT ["dotnet", "Dsw2025Tpi.Api.dll"]