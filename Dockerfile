# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN --mount=type=secret,id=proxy_ca \
    if [ -f /run/secrets/proxy_ca ]; then cp /run/secrets/proxy_ca /usr/local/share/ca-certificates/session-proxy.crt; update-ca-certificates; fi; \
    dotnet restore src/FlashSale.Api && \
    dotnet publish src/FlashSale.Api -c Release --no-restore -o /app && \
    rm -f /usr/local/share/ca-certificates/session-proxy.crt && update-ca-certificates
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "FlashSale.Api.dll"]
