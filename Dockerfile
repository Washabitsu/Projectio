# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first: this layer is reused until the package list changes
COPY Projectio/Projectio.csproj Projectio/
RUN dotnet restore Projectio/Projectio.csproj

COPY Projectio/ Projectio/
RUN dotnet publish Projectio/Projectio.csproj -c Release -o /app/publish --no-restore

# Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Plain HTTP inside the container: HTTPS ends at the reverse proxy in front of it
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# The non-root user that the .NET images ship with
USER $APP_UID

ENTRYPOINT ["dotnet", "Projectio.dll"]
