# ---------- Stage 1: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY *.csproj .
RUN dotnet restore

# Copy everything else
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ---------- Stage 2: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "CloudMediaHub.Api.dll"]
