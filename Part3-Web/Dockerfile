# ---------- Stage 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first, so this layer is cached until the project file changes
COPY RaceDay.Web.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish RaceDay.Web.csproj -c Release -o /app/publish --no-restore

# ---------- Stage 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The API address can be changed at run time: -e ApiBaseUrl=...
ENV ASPNETCORE_URLS=http://+:8080
ENV ApiBaseUrl=http://host.docker.internal:5218
EXPOSE 8080

# Run as a non-root user
USER app

ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]
