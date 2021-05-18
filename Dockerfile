FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./StreamBadger.sln .
COPY ./src/StreamBadger/StreamBadger.csproj ./src/StreamBadger/
COPY ./src/StreamBadgerDesktop/StreamBadgerDesktop.csproj ./src/StreamBadgerDesktop/
COPY ./src/StreamBadgerLogin/StreamBadgerLogin.csproj ./src/StreamBadgerLogin/
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish --no-restore -c Release -o out ./src/StreamBadgerLogin

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["./StreamBadgerLogin"]
