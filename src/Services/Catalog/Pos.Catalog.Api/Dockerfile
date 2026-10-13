# syntax=docker/dockerfile:1
# Build context: repository root.  docker build -f src/Services/Catalog/Pos.Catalog.Api/Dockerfile -t pos/catalog-api:latest .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source

COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/ src/
RUN dotnet restore "src/Services/Catalog/Pos.Catalog.Api/Pos.Catalog.Api.csproj"
RUN dotnet publish "src/Services/Catalog/Pos.Catalog.Api/Pos.Catalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080
COPY --from=build /app/publish .
# Run as the image's built-in non-root user.
USER $APP_UID
ENTRYPOINT ["dotnet", "Pos.Catalog.Api.dll"]
