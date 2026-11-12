# Build from the repo root: docker build -f src/CommonGround.Management.Gateway/Dockerfile .
# The image holds the gateway and the built management web app, which it serves from wwwroot.
FROM node:24-alpine AS web
WORKDIR /web
COPY src/CommonGround.Management.Web/package.json src/CommonGround.Management.Web/package-lock.json ./
RUN npm ci
COPY src/CommonGround.Management.Web/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY src/ src/
RUN dotnet publish src/CommonGround.Management.Gateway/CommonGround.Management.Gateway.csproj -c Release -o /app --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
COPY --from=web /web/dist ./wwwroot
USER $APP_UID
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "CommonGround.Management.Gateway.dll"]
