# ===== Stage 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the csproj first so Docker can cache the package restore
# layer — subsequent builds skip re-downloading packages unless the
# .csproj itself changed, which speeds up every rebuild significantly.
COPY *.csproj ./
RUN dotnet restore

# Now copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore

# ===== Stage 2: Runtime =====
# Much smaller image — no SDK, no build tools, just what's needed to run.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Render provides the actual port via the PORT environment variable —
# ASP.NET Core needs to be told to listen on it explicitly.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SA Builders.dll"]