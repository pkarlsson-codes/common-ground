# Build from the repo root: docker build -f src/CommonGround.API/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY src/ src/
RUN dotnet publish src/CommonGround.API/CommonGround.API.csproj -c Release -o /app --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
# Documents:FileSystem:RootPath defaults to data/documents under /app. Mount a volume there.
# The directory is created here so a new named volume takes the app user's ownership.
RUN mkdir -p /app/data/documents && chown -R $APP_UID /app/data
USER $APP_UID
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "CommonGround.API.dll"]
