# Build the Angular client first so the published ASP.NET image can serve it from wwwroot.
FROM node:24-bookworm-slim AS client-build
WORKDIR /src/league_trader_temple.client

COPY league_trader_temple.client/package*.json ./
RUN npm ci --no-audit --no-fund

COPY league_trader_temple.client/ ./
RUN npm run build

# Restore and publish the ASP.NET Core server.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS server-build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY League_Trader_Temple.Server/League_Trader_Temple.Server.csproj League_Trader_Temple.Server/
COPY league_trader_temple.client/league_trader_temple.client.esproj league_trader_temple.client/
RUN dotnet restore League_Trader_Temple.Server/League_Trader_Temple.Server.csproj /p:BuildingInsideDocker=true

COPY . ./
COPY --from=client-build /src/league_trader_temple.client/dist/league_trader_temple.client/browser/ League_Trader_Temple.Server/wwwroot/

WORKDIR /src/League_Trader_Temple.Server
RUN dotnet publish League_Trader_Temple.Server.csproj \
    --configuration $BUILD_CONFIGURATION \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:BuildingInsideDocker=true

# Run the published app on the smaller ASP.NET runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=server-build /app/publish ./
USER $APP_UID

ENTRYPOINT ["dotnet", "League_Trader_Temple.Server.dll"]
