# ===== Stage 1: Build =====
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all source files and publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore

# ===== Stage 2: Runtime =====
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Enable invariant globalization to prevent Linux C-library crashes
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Instruct ASP.NET Core to bind dynamically to the PORT assigned by Render
ENV ASPNETCORE_HTTP_PORTS=${PORT:-8080}

ENTRYPOINT ["dotnet", "SA Builders.dll"]